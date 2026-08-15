using System.Linq.Expressions;
using FreeGency.AI.HirePyInterview;
using FreeGency.AI.HirePyInterview.MilestonePlanning;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.HirePy;
using FreeGency.Application.Features.Milestones.DTOs;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FreeGency.Tests;

public class HirePyInterviewServiceTests
{
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();
    private readonly Guid _freelancerUserId = Guid.NewGuid();
    private readonly Guid _freelancerProfileId = Guid.NewGuid();
    private readonly Guid _botUserId = Guid.NewGuid();
    private readonly Guid _botProfileId = Guid.NewGuid();
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _proposalId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();

    private sealed record Fixture(
        HirePyInterviewService Service,
        HirePyInterview Interview,
        HirePySession Session,
        Project Project,
        ProjectProposal Proposal,
        Mock<IHirePyInterviewAgent> Agent,
        Mock<IHirePyMilestonePlannerAgent> PlannerAgent,
        Mock<IMilestoneService> MilestoneService,
        Mock<IHirePyEventPublisher> EventPublisher,
        Mock<INotificationService> NotificationService,
        Mock<IBackgroundJobClient> BackgroundJobClient,
        Mock<IUserRepository> UserRepo,
        Mock<IDeveloperProfileRepository> DeveloperProfileRepo,
        Mock<IMessageRepository> MessageRepo,
        Mock<IChatRoomMemberRepository> RoomMemberRepo,
        Mock<IChatRoomRepository> ChatRoomRepo,
        Mock<IHirePyInterviewRepository> InterviewRepo,
        Mock<IClientProxy> ClientProxy);

    [Fact]
    public async Task EnsureInterviewAsync_CreatesPrivateAiRoom_WithFreelancerAndBotOnly_AndEnqueuesOpening()
    {
        var f = Build();

        ChatRoom? capturedRoom = null;
        List<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>? capturedMembers = null;
        f.ChatRoomRepo.Setup(r => r.AddWithMembersAsync(
                It.IsAny<ChatRoom>(),
                It.IsAny<IEnumerable<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ChatRoom, IEnumerable<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>, CancellationToken>(
                (room, members, _) =>
                {
                    capturedRoom = room;
                    capturedMembers = members.ToList();
                })
            .Returns(Task.CompletedTask);

        HirePyInterview? capturedInterview = null;
        f.InterviewRepo.Setup(r => r.AddAsync(It.IsAny<HirePyInterview>(), It.IsAny<CancellationToken>()))
            .Callback<HirePyInterview, CancellationToken>((i, _) => capturedInterview = i)
            .Returns(Task.CompletedTask);

        await f.Service.EnsureInterviewAsync(_sessionId, _proposalId, _freelancerUserId, CancellationToken.None);

        Assert.NotNull(capturedRoom);
        Assert.Equal("HirePy AI Interview", capturedRoom!.Title);
        Assert.Equal(RoomType.Proposal, capturedRoom.RoomType);
        Assert.Equal(ChatRoomStatus.Active, capturedRoom.Status);
        Assert.Null(capturedRoom.ProjectId);
        Assert.Null(capturedRoom.ProposalId);

        Assert.NotNull(capturedMembers);
        Assert.Equal(2, capturedMembers!.Count);
        Assert.DoesNotContain(capturedMembers, m => m.ClientProfileId.HasValue);
        Assert.Contains(capturedMembers, m => m.DeveloperProfileId == _freelancerProfileId && m.RoleLabel == "Candidate" && m.CanSend);
        Assert.Contains(capturedMembers, m => m.DeveloperProfileId == _botProfileId && m.RoleLabel == "HirePy AI" && m.CanSend);

        Assert.NotNull(capturedInterview);
        Assert.Equal(_sessionId, capturedInterview!.HirePySessionId);
        Assert.Equal(_proposalId, capturedInterview.ProjectProposalId);
        Assert.Equal(capturedRoom!.Id, capturedInterview.ChatRoomId);
        Assert.Equal(_freelancerProfileId, capturedInterview.FreelancerDeveloperProfileId);
        Assert.Equal(HirePyInterviewStatus.Started, capturedInterview.Status);

        f.BackgroundJobClient.Verify(
            b => b.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
    }

    [Fact]
    public async Task EnsureInterviewAsync_WhenInterviewExists_IsIdempotent()
    {
        var f = Build();
        f.InterviewRepo.Setup(r => r.GetByProposalIdAsync(_proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HirePyInterview { Id = Guid.NewGuid(), ProjectProposalId = _proposalId });

        await f.Service.EnsureInterviewAsync(_sessionId, _proposalId, _freelancerUserId, CancellationToken.None);

        f.ChatRoomRepo.Verify(r => r.AddWithMembersAsync(
            It.IsAny<ChatRoom>(),
            It.IsAny<IEnumerable<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        f.BackgroundJobClient.Verify(
            b => b.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_OpeningTurn_WritesAiMessageAndBroadcastsToFreelancer()
    {
        var f = Build();
        SetTwoMemberRoom(f);

        Message? aiMessage = null;
        f.MessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => aiMessage = m)
            .Returns(Task.CompletedTask);

        await f.Service.ProcessInterviewAsync(f.Interview.Id, CancellationToken.None);

        Assert.NotNull(aiMessage);
        Assert.Equal(_roomId, aiMessage!.ChatRoomId);
        Assert.Equal(_botProfileId, aiMessage.SenderDeveloperProfileId);
        Assert.Equal("Tell me more about your architecture.", aiMessage.Text);
        Assert.Null(aiMessage.SenderClientProfileId);

        Assert.Equal(1, f.Interview.TurnCount);
        Assert.Equal(HirePyInterviewStatus.Started, f.Interview.Status);
        Assert.Null(f.Interview.ProcessingAt);

        f.ClientProxy.Verify(p => p.SendCoreAsync(
            "ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        f.ClientProxy.Verify(p => p.SendCoreAsync(
            "RoomUpdated", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);

        f.BackgroundJobClient.Verify(
            b => b.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.DiscussionStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyDiscussionStarted)), Times.Once);
    }

    [Fact]
    public async Task ProcessInterviewAsync_FollowUp_AnswersOnlyTheNewCandidateMessage()
    {
        var messages = new List<Message>
        {
            BotMessage(Guid.NewGuid(), "Welcome, tell me about your experience.", DateTime.UtcNow.AddMinutes(-3)),
            CandidateMessage(Guid.NewGuid(), "I have built 3 marketplaces.", DateTime.UtcNow.AddMinutes(-2)),
            CandidateMessage(Guid.NewGuid(), "My tech stack is React and Node.", DateTime.UtcNow.AddMinutes(-1))
        };
        var interview = Interview(StartedInterview());
        interview.TurnCount = 1;
        interview.LastUserMessageId = messages[1].Id;
        var f = Build(interview, messages);

        Message? aiMessage = null;
        f.MessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => aiMessage = m)
            .Returns(Task.CompletedTask);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.NotNull(aiMessage);
        Assert.Equal(2, interview.TurnCount);
        Assert.Equal(messages[2].Id, interview.LastUserMessageId);
        f.Agent.Verify(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessInterviewAsync_NoNewCandidateMessage_SkipsTurn()
    {
        var messages = new List<Message>
        {
            BotMessage(Guid.NewGuid(), "Welcome, tell me about your experience.", DateTime.UtcNow.AddMinutes(-2)),
            CandidateMessage(Guid.NewGuid(), "I have built 3 marketplaces.", DateTime.UtcNow.AddMinutes(-1))
        };
        var interview = Interview(StartedInterview());
        interview.TurnCount = 1;
        interview.LastUserMessageId = messages[1].Id;
        var f = Build(interview, messages);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        f.Agent.Verify(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()), Times.Never);
        f.MessageRepo.Verify(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_ActiveClaim_SkipsConcurrentProcessing()
    {
        var interview = Interview(StartedInterview());
        interview.ProcessingAt = DateTime.UtcNow.AddSeconds(-30);
        var f = Build(interview, []);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        f.Agent.Verify(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_StaleClaim_ResumesAndAnswersPendingMessage()
    {
        var messages = new List<Message>
        {
            BotMessage(Guid.NewGuid(), "Welcome.", DateTime.UtcNow.AddMinutes(-5)),
            CandidateMessage(Guid.NewGuid(), "I use Azure.", DateTime.UtcNow.AddMinutes(-4))
        };
        var interview = Interview(StartedInterview());
        interview.TurnCount = 1;
        interview.LastUserMessageId = null;
        interview.ProcessingAt = DateTime.UtcNow.AddMinutes(-10);
        var f = Build(interview, messages);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(messages[1].Id, interview.LastUserMessageId);
        Assert.Equal(2, interview.TurnCount);
        Assert.Null(interview.ProcessingAt);
    }

    [Fact]
    public async Task ProcessInterviewAsync_MilestoneDecision_RequestsMilestonePlanAndNotifiesClient()
    {
        var messages = new List<Message>
        {
            BotMessage(Guid.NewGuid(), "Welcome.", DateTime.UtcNow.AddMinutes(-2)),
            CandidateMessage(Guid.NewGuid(), "Approach is clear.", DateTime.UtcNow.AddMinutes(-1))
        };
        var interview = Interview(StartedInterview());
        interview.TurnCount = 1;
        interview.LastUserMessageId = null;
        var f = Build(interview, messages);
        f.Agent.Setup(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HirePyInterviewReply
            {
                Message = "Great. Please send a milestone plan.",
                Decision = HirePyInterviewDecision.RequestMilestonePlan
            });

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(HirePyInterviewStatus.MilestonePlanRequested, interview.Status);
        Assert.Null(interview.ConcludedAt);

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestonePlanRequested, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestonePlanningStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyMilestonePlanRequested)), Times.Once);
    }

    [Fact]
    public async Task ProcessInterviewAsync_AiFailure_RetriesThenFailsAfterBoundedFailures()
    {
        var f = Build(Interview(StartedInterview()), []);
        f.Agent.Setup(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI gateway down"));

        await f.Service.ProcessInterviewAsync(f.Interview.Id, CancellationToken.None);
        Assert.Equal(HirePyInterviewStatus.Started, f.Interview.Status);
        Assert.Equal(1, f.Interview.FailureCount);
        Assert.Null(f.Interview.ProcessingAt);

        await f.Service.ProcessInterviewAsync(f.Interview.Id, CancellationToken.None);
        Assert.Equal(HirePyInterviewStatus.Started, f.Interview.Status);
        Assert.Equal(2, f.Interview.FailureCount);

        await f.Service.ProcessInterviewAsync(f.Interview.Id, CancellationToken.None);
        Assert.Equal(HirePyInterviewStatus.Failed, f.Interview.Status);
        Assert.Equal(3, f.Interview.FailureCount);
        Assert.NotNull(f.Interview.ConcludedAt);

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.DiscussionFailed, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingAsync_ProcessesDueInterviews()
    {
        var f = Build();
        f.InterviewRepo.Setup(r => r.GetPendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([f.Interview]);

        await f.Service.ProcessPendingAsync(CancellationToken.None);

        f.Agent.Verify(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, f.Interview.TurnCount);
    }

    private void SetTwoMemberRoom(Fixture f)
    {
        f.RoomMemberRepo.Setup(r => r.GetRoomProfileIdsAsync(_roomId))
            .ReturnsAsync(
            [
                new ChatRoomMember { ChatRoomId = _roomId, DeveloperProfileId = _freelancerProfileId },
                new ChatRoomMember { ChatRoomId = _roomId, DeveloperProfileId = _botProfileId }
            ]);
    }

    private HirePyInterview PlanningInterview(HirePyInterviewStatus status, int planningRounds = 0, Guid? lastUserMessageId = null)
    {
        var interview = Interview(StartedInterview());
        interview.Status = status;
        interview.PlanningRounds = planningRounds;
        interview.LastUserMessageId = lastUserMessageId;
        interview.TurnCount = 1;
        return interview;
    }

    private (Guid PlanMessageId, List<Message> Messages) PlanMessages(DateTime planAt)
    {
        var planId = Guid.NewGuid();
        return (planId, new List<Message>
        {
            BotMessage(Guid.NewGuid(), "Please send your milestone plan.", planAt.AddMinutes(-5)),
            CandidateMessage(planId, "Milestone 1: payment integration, 2 days, 300. Milestone 2: reporting, 3 days, 500.", planAt)
        });
    }

    private void SetupPlannerFinalize(Fixture f, bool success = true)
    {
        f.PlannerAgent.Setup(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MilestonePlanningReply
            {
                Message = "The plan looks good. Finalizing.",
                Outcome = MilestonePlanningOutcome.Finalize,
                Milestones =
                [
                    new ProposedMilestone
                    {
                        Title = "Payment integration",
                        DefinitionOfDone = "Stripe checkout wired",
                        Amount = 300,
                        DueDate = DateTime.UtcNow.Date.AddDays(2),
                        SortOrder = 1
                    },
                    new ProposedMilestone
                    {
                        Title = "Reporting",
                        DefinitionOfDone = "Sales dashboard",
                        Amount = 500,
                        DueDate = DateTime.UtcNow.Date.AddDays(5),
                        SortOrder = 2
                    }
                ]
            });

        f.MilestoneService.Setup(m => m.ProposePlanAsync(It.IsAny<ProposeMilestonePlanDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(success
                ? ApiResponse.Success(new MilestonePlanVersionDto
                {
                    Id = Guid.NewGuid(),
                    ProjectId = _projectId,
                    ProposalId = _proposalId,
                    Version = 1,
                    Status = "Proposed",
                    ProposedByUserId = _freelancerUserId
                }, "Milestone plan v1 proposed.")
                : ApiResponse.Failure<MilestonePlanVersionDto>(
                    AppError.Validation("Budget exceeds the proposal."), "Budget exceeds the proposal."));
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_NoNewPlanMessage_Skips()
    {
        var (planId, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-1));
        var interview = PlanningInterview(HirePyInterviewStatus.MilestonePlanRequested, lastUserMessageId: planId);
        var f = Build(interview, messages);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        f.PlannerAgent.Verify(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()), Times.Never);
        f.MessageRepo.Verify(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_ReceivesPlan_RequestsRevisionWithEvents()
    {
        var (planId, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-1));
        var interview = PlanningInterview(HirePyInterviewStatus.MilestonePlanRequested, lastUserMessageId: null);
        var f = Build(interview, messages);
        SetTwoMemberRoom(f);

        Message? aiMessage = null;
        f.MessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => aiMessage = m)
            .Returns(Task.CompletedTask);

        MilestonePlanningContext? context = null;
        f.PlannerAgent.Setup(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()))
            .Callback<MilestonePlanningContext, CancellationToken>((c, _) => context = c)
            .ReturnsAsync(new MilestonePlanningReply
            {
                Message = "Please split the payment milestone into smaller steps and explain the estimate.",
                Outcome = MilestonePlanningOutcome.NeedsRevision,
                Issues = ["Unrealistic 2-day estimate for payment integration."]
            });

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(HirePyInterviewStatus.MilestoneRevisionRequested, interview.Status);
        Assert.Equal(1, interview.PlanningRounds);
        Assert.Equal(planId, interview.LastUserMessageId);
        Assert.Equal(2, interview.TurnCount);
        Assert.Null(interview.ProcessingAt);

        Assert.NotNull(aiMessage);
        Assert.Equal("Please split the payment milestone into smaller steps and explain the estimate.", aiMessage!.Text);
        Assert.DoesNotContain("Plan note:", aiMessage.Text);

        Assert.NotNull(context);
        Assert.Equal("Layla Farid", context!.CandidateName);
        Assert.False(context.IsFinalAttempt);
        Assert.Contains(context.History, h => h.Content.Contains("payment integration"));

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestonePlanReceived, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestoneReviewStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestoneRevisionRequested, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.MilestoneService.Verify(m => m.ProposePlanAsync(It.IsAny<ProposeMilestonePlanDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_Finalize_PersistsPlanViaExistingMilestoneFlow()
    {
        var (planId, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-1));
        var interview = PlanningInterview(HirePyInterviewStatus.MilestonePlanRequested, lastUserMessageId: null);
        var f = Build(interview, messages);
        SetTwoMemberRoom(f);
        SetupPlannerFinalize(f);

        ProposeMilestonePlanDto? captured = null;
        f.MilestoneService.Setup(m => m.ProposePlanAsync(It.IsAny<ProposeMilestonePlanDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<ProposeMilestonePlanDto, Guid, CancellationToken>((dto, userId, _) => captured = dto)
            .ReturnsAsync(ApiResponse.Success(new MilestonePlanVersionDto
            {
                Id = Guid.NewGuid(),
                ProjectId = _projectId,
                ProposalId = _proposalId,
                Version = 1,
                Status = "Proposed",
                ProposedByUserId = _freelancerUserId
            }, "Milestone plan v1 proposed."));

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(HirePyInterviewStatus.MilestonePlanFinalized, interview.Status);
        Assert.NotNull(interview.ConcludedAt);
        Assert.Equal(1, interview.PlanningRounds);
        Assert.Equal(planId, interview.LastUserMessageId);

        Assert.NotNull(captured);
        Assert.Equal(_projectId, captured!.ProjectId);
        Assert.Equal(_proposalId, captured.ProposalId);
        Assert.Equal(2, captured.Milestones.Count);
        Assert.Equal("Payment integration", captured.Milestones[0].Title);
        Assert.Equal(300m, captured.Milestones[0].Amount);
        Assert.Equal(500m, captured.Milestones[1].Amount);

        f.MilestoneService.Verify(m => m.ProposePlanAsync(
            It.IsAny<ProposeMilestonePlanDto>(), _freelancerUserId, It.IsAny<CancellationToken>()), Times.Once);

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestonePlanFinalized, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyMilestonePlanFinalized)), Times.Once);
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_ProposeFailure_RequestsRevisionWithPlanNote()
    {
        var (_, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-1));
        var interview = PlanningInterview(HirePyInterviewStatus.MilestonePlanRequested, lastUserMessageId: null);
        var f = Build(interview, messages);
        SetTwoMemberRoom(f);
        SetupPlannerFinalize(f, success: false);

        Message? aiMessage = null;
        f.MessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => aiMessage = m)
            .Returns(Task.CompletedTask);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(HirePyInterviewStatus.MilestoneRevisionRequested, interview.Status);
        Assert.NotNull(aiMessage);
        Assert.Contains("Plan note: Budget exceeds the proposal.", aiMessage!.Text);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.MilestonePlanFinalized, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_FinalizeWithEmptyMilestones_TreatedAsRevision()
    {
        var (_, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-1));
        var interview = PlanningInterview(HirePyInterviewStatus.MilestonePlanRequested, lastUserMessageId: null);
        var f = Build(interview, messages);
        f.PlannerAgent.Setup(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MilestonePlanningReply
            {
                Message = "Finalizing but the list was empty.",
                Outcome = MilestonePlanningOutcome.Finalize
            });

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(HirePyInterviewStatus.MilestoneRevisionRequested, interview.Status);
        f.MilestoneService.Verify(m => m.ProposePlanAsync(It.IsAny<ProposeMilestonePlanDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_FinalAttemptRevision_FailsInterview()
    {
        var (_, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-1));
        var interview = PlanningInterview(HirePyInterviewStatus.MilestoneRevisionRequested, planningRounds: 6, lastUserMessageId: null);
        var f = Build(interview, messages);
        f.PlannerAgent.Setup(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()))
            .Callback<MilestonePlanningContext, CancellationToken>((c, _) => Assert.True(c.IsFinalAttempt))
            .ReturnsAsync(new MilestonePlanningReply
            {
                Message = "Still needs work, but this was the last round.",
                Outcome = MilestonePlanningOutcome.NeedsRevision
            });

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);

        Assert.Equal(HirePyInterviewStatus.Failed, interview.Status);
        Assert.NotNull(interview.FailReason);
        Assert.NotNull(interview.ConcludedAt);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.DiscussionFailed, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessInterviewAsync_PlanningTurn_MultipleRounds_NegotiatesThenFinalizes()
    {
        var (firstPlanId, messages) = PlanMessages(DateTime.UtcNow.AddMinutes(-2));

        var interview = PlanningInterview(HirePyInterviewStatus.MilestonePlanRequested, lastUserMessageId: null);
        var f = Build(interview, messages);
        SetTwoMemberRoom(f);

        var rounds = 0;
        f.PlannerAgent.Setup(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MilestonePlanningContext c, CancellationToken _) =>
            {
                rounds++;
                return rounds == 1
                    ? new MilestonePlanningReply
                    {
                        Message = "Please revise the payment estimate.",
                        Outcome = MilestonePlanningOutcome.NeedsRevision
                    }
                    : new MilestonePlanningReply
                    {
                        Message = "Accepted.",
                        Outcome = MilestonePlanningOutcome.Finalize,
                        Milestones =
                        [
                            new ProposedMilestone { Title = "Payment", DefinitionOfDone = "x", Amount = 350, SortOrder = 1 },
                            new ProposedMilestone { Title = "Reporting", DefinitionOfDone = "y", Amount = 450, SortOrder = 2 }
                        ]
                    };
            });
        f.MilestoneService.Setup(m => m.ProposePlanAsync(It.IsAny<ProposeMilestonePlanDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success(new MilestonePlanVersionDto
            {
                Id = Guid.NewGuid(), ProjectId = _projectId, ProposalId = _proposalId, Version = 1, Status = "Proposed"
            }, "ok"));

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);
        Assert.Equal(HirePyInterviewStatus.MilestoneRevisionRequested, interview.Status);
        Assert.Equal(1, interview.PlanningRounds);
        Assert.Equal(firstPlanId, interview.LastUserMessageId);

        var secondPlan = CandidateMessage(Guid.NewGuid(), "Updated: payment 3 days 350, reporting 3 days 450.", DateTime.UtcNow.AddMinutes(-1));
        messages.Add(secondPlan);

        await f.Service.ProcessInterviewAsync(interview.Id, CancellationToken.None);
        Assert.Equal(HirePyInterviewStatus.MilestonePlanFinalized, interview.Status);
        Assert.Equal(2, interview.PlanningRounds);
        Assert.Equal(secondPlan.Id, interview.LastUserMessageId);
        f.MilestoneService.Verify(m => m.ProposePlanAsync(It.IsAny<ProposeMilestonePlanDto>(), _freelancerUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private HirePyInterview StartedInterview()
        => new()
        {
            Id = Guid.NewGuid(),
            HirePySessionId = _sessionId,
            ProjectId = _projectId,
            ProjectProposalId = _proposalId,
            ChatRoomId = _roomId,
            FreelancerUserId = _freelancerUserId,
            FreelancerDeveloperProfileId = _freelancerProfileId,
            Status = HirePyInterviewStatus.Started,
            TurnCount = 0,
            CreatedAt = DateTime.UtcNow
        };

    private HirePyInterview Interview(HirePyInterview interview) => interview;

    private Message BotMessage(Guid id, string text, DateTime createdAt)
        => new()
        {
            Id = id,
            ChatRoomId = _roomId,
            SenderDeveloperProfileId = _botProfileId,
            Text = text,
            MessageType = MessageType.Text,
            ModerationStatus = ModerationStatus.Visible,
            CreatedAt = createdAt
        };

    private Message CandidateMessage(Guid id, string text, DateTime createdAt)
        => new()
        {
            Id = id,
            ChatRoomId = _roomId,
            SenderDeveloperProfileId = _freelancerProfileId,
            Text = text,
            MessageType = MessageType.Text,
            ModerationStatus = ModerationStatus.Visible,
            CreatedAt = createdAt
        };

    private Fixture Build(HirePyInterview? interview = null, IReadOnlyList<Message>? messages = null)
    {
        var theInterview = interview ?? StartedInterview();
        var session = new HirePySession
        {
            Id = _sessionId,
            ClientUserId = _clientUserId,
            ProjectId = _projectId,
            Status = HirePySessionStatus.CandidateDiscussion,
            CreatedAt = DateTime.UtcNow
        };
        var project = new Project
        {
            Id = _projectId,
            Title = "Bakery Website",
            Description = "Build a bakery website.",
            Currency = "USD",
            BudgetMin = 1000,
            BudgetMax = 2000,
            Deadline = DateTime.UtcNow.AddMonths(1),
            EstimatedDurationDays = 30
        };
        var proposal = new ProjectProposal
        {
            Id = _proposalId,
            ProjectId = _projectId,
            UserId = _freelancerUserId,
            CoverLetter = "I would love to build this.",
            Approach = "React + Node",
            ProposedTimeline = "3 weeks",
            ProposedBudget = 1500
        };

        var sessionRepo = new Mock<IHirePySessionRepository>();
        var interviewRepo = new Mock<IHirePyInterviewRepository>();
        var projectRepo = new Mock<IProjectRepository>();
        var proposalRepo = new Mock<IProjectProposalRepository>();
        var userRepo = new Mock<IUserRepository>();
        var developerProfileRepo = new Mock<IDeveloperProfileRepository>();
        var chatRoomRepo = new Mock<IChatRoomRepository>();
        var roomMemberRepo = new Mock<IChatRoomMemberRepository>();
        var messageRepo = new Mock<IMessageRepository>();

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IHirePySessionRepository, HirePySession>()).Returns(sessionRepo.Object);
        unitOfWork.Setup(u => u.Repository<IHirePyInterviewRepository, HirePyInterview>()).Returns(interviewRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectRepository, Project>()).Returns(projectRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectProposalRepository, ProjectProposal>()).Returns(proposalRepo.Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.Repository<IDeveloperProfileRepository, DeveloperProfile>()).Returns(developerProfileRepo.Object);
        unitOfWork.Setup(u => u.Repository<IChatRoomRepository, ChatRoom>()).Returns(chatRoomRepo.Object);
        unitOfWork.Setup(u => u.Repository<IChatRoomMemberRepository, ChatRoomMember>()).Returns(roomMemberRepo.Object);
        unitOfWork.Setup(u => u.Repository<IMessageRepository, Message>()).Returns(messageRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        sessionRepo.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        projectRepo.Setup(r => r.GetByIdAsync(_projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        proposalRepo.Setup(r => r.GetByIdAsync(_proposalId, It.IsAny<CancellationToken>())).ReturnsAsync(proposal);
        interviewRepo.Setup(r => r.GetByIdAsync(theInterview.Id, It.IsAny<CancellationToken>())).ReturnsAsync(theInterview);
        interviewRepo.Setup(r => r.GetByProposalIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((HirePyInterview?)null);
        userRepo.Setup(r => r.GetByIdAsync(_freelancerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _freelancerUserId, FristName = "Layla", LastName = "Farid" });
        userRepo.Setup(r => r.GetDeveloperProfileIdByUserIdAsync(_freelancerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_freelancerProfileId);
        userRepo.Setup(r => r.GetClientProfileIdByUserIdAsync(_clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_clientProfileId);
        developerProfileRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<DeveloperProfile>(
            [new DeveloperProfile { Id = _botProfileId, UserId = _botUserId, IsHirePyBot = true, IsDeleted = false }]));
        messageRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Message>(messages ?? []));
        roomMemberRepo.Setup(r => r.GetRoomProfileIdsAsync(It.IsAny<Guid>())).ReturnsAsync([]);

        var agent = new Mock<IHirePyInterviewAgent>();
        agent.Setup(a => a.GetNextReplyAsync(It.IsAny<HirePyInterviewContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HirePyInterviewReply
            {
                Message = "Tell me more about your architecture.",
                Decision = HirePyInterviewDecision.AskQuestion
            });

        var plannerAgent = new Mock<IHirePyMilestonePlannerAgent>();
        plannerAgent.Setup(a => a.PlanNextStepAsync(It.IsAny<MilestonePlanningContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MilestonePlanningReply
            {
                Message = "Please split the payment integration milestone into smaller steps.",
                Outcome = MilestonePlanningOutcome.NeedsRevision
            });

        var milestoneService = new Mock<IMilestoneService>();

        var eventPublisher = new Mock<IHirePyEventPublisher>();
        var notificationService = new Mock<INotificationService>();
        var backgroundJobClient = new Mock<IBackgroundJobClient>();

        var hub = new Mock<IHubContext<ChatHub>>();
        var hubClients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();
        clientProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);
        hub.Setup(h => h.Clients).Returns(hubClients.Object);

        var passwordHasher = new Mock<IPasswordHasher<User>>();
        passwordHasher.Setup(p => p.HashPassword(It.IsAny<User>(), It.IsAny<string>())).Returns("HASHED");

        var service = new HirePyInterviewService(
            unitOfWork.Object,
            agent.Object,
            plannerAgent.Object,
            milestoneService.Object,
            eventPublisher.Object,
            notificationService.Object,
            hub.Object,
            backgroundJobClient.Object,
            passwordHasher.Object);

        return new Fixture(
            service, theInterview, session, project, proposal, agent, plannerAgent, milestoneService,
            eventPublisher, notificationService, backgroundJobClient, userRepo, developerProfileRepo,
            messageRepo, roomMemberRepo, chatRoomRepo, interviewRepo, clientProxy);
    }
}
