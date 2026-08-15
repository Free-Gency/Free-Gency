using System.Text.Json;
using FreeGency.AI.HirePyInterview.Evaluation;
using FreeGency.AI.Ranking.ProposalRanking;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.HirePy;
using FreeGency.Application.Features.HirePy.Dtos;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Interfaces;
using Moq;

namespace FreeGency.Tests;

public class HirePyEvaluationServiceTests
{
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _freelancerAUserId = Guid.NewGuid();
    private readonly Guid _freelancerBUserId = Guid.NewGuid();
    private readonly Guid _freelancerAProfileId = Guid.NewGuid();
    private readonly Guid _freelancerBProfileId = Guid.NewGuid();
    private readonly Guid _roomAId = Guid.NewGuid();
    private readonly Guid _roomBId = Guid.NewGuid();

    private sealed record Fixture(
        HirePyEvaluationService Service,
        HirePySession Session,
        Mock<IHirePySessionRepository> SessionRepo,
        Mock<IHirePyInterviewRepository> InterviewRepo,
        Mock<IHirePyEvaluationRepository> EvaluationRepo,
        Mock<IProjectRepository> ProjectRepo,
        Mock<IProjectProposalRepository> ProposalRepo,
        Mock<IUserRepository> UserRepo,
        Mock<IMessageRepository> MessageRepo,
        Mock<IMilestonePlanVersionRepository> PlanVersionRepo,
        Mock<IHirePyEvaluatorAgent> Agent,
        Mock<IProposalRankingService> RankingService,
        Mock<IHirePyEventPublisher> EventPublisher,
        Mock<INotificationService> NotificationService,
        Mock<ICurrentUserService> CurrentUser);

    [Fact]
    public async Task ProcessPendingEvaluations_Success_EvaluatesAllFinalizedAndSelectsWinner()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId),
            Interview(_roomBId, proposalB.Id, _freelancerBUserId, _freelancerBProfileId)
        };
        var f = Build(interviews, [proposalA, proposalB], SelectedCandidatesJson(proposalA, proposalB));

        f.Agent.Setup(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()))
            .Returns((HirePyEvaluationContext ctx, CancellationToken _) =>
                Task.FromResult(Reply(ctx.CandidateName == "Dev A" ? 75 : 90)));

        var added = CaptureEvaluations(f);

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        Assert.Equal(proposalB.Id, f.Session.SelectedProposalId);
        Assert.Equal(_freelancerBUserId, f.Session.SelectedFreelancerUserId);
        Assert.Equal("Dev B", f.Session.SelectedCandidateName);
        Assert.Equal("Decision reason for Dev B.", f.Session.DecisionReason);
        Assert.Contains("Payment integration", f.Session.MilestoneSummary);
        Assert.Equal(1800m, f.Session.SelectedBudget);
        Assert.Equal("4 weeks", f.Session.SelectedTimeline);
        Assert.NotNull(f.Session.RecommendationCompletedAt);

        Assert.Equal(2, added.Count);

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateEvaluationStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateEvaluationCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateComparisonStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.RecommendationReady, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.WaitingForClientApproval, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyRecommendationReady)), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_Selection_DoesNotFollowOriginalRanking()
    {
        // Dev A is rank 1 (best original ranking) but the AI evaluation scores Dev B higher
        // based on the interaction and the milestone plan -> Dev B must win.
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId),
            Interview(_roomBId, proposalB.Id, _freelancerBUserId, _freelancerBProfileId)
        };
        var f = Build(interviews, [proposalA, proposalB], SelectedCandidatesJson(proposalA, proposalB));

        f.Agent.Setup(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()))
            .Returns((HirePyEvaluationContext ctx, CancellationToken _) =>
                Task.FromResult(Reply(ctx.CandidateName == "Dev A" ? 55 : 92)));

        var added = CaptureEvaluations(f);

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        Assert.Equal(proposalB.Id, f.Session.SelectedProposalId);
        Assert.Equal("Dev B", f.Session.SelectedCandidateName);

        var devA = added.Single(e => e.FreelancerUserId == _freelancerAUserId);
        var devB = added.Single(e => e.FreelancerUserId == _freelancerBUserId);
        Assert.Equal(1, devA.RankingPosition);
        Assert.Equal(2, devB.RankingPosition);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_WhenInterviewStillActive_SkipsSession()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId,
                HirePyInterviewStatus.Started)
        };
        var f = Build(interviews, [proposalA], null);

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.CandidateDiscussion, f.Session.Status);
        f.Agent.Verify(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()), Times.Never);
        f.NotificationService.Verify(n => n.CreateNotification(It.IsAny<CreateNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_WhenNoInterviewFinalized_MarksFailed()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId,
                HirePyInterviewStatus.Failed)
        };
        var f = Build(interviews, [proposalA], null);

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, f.Session.Status);
        Assert.Contains("No candidate finalized", f.Session.FailReason);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateEvaluationFailed, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyFailed)), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_WhenAiOutputInvalid_MarksFailed()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId)
        };
        var f = Build(interviews, [proposalA], null);

        f.Agent.Setup(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HirePyEvaluationReply { IsValid = false });

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, f.Session.Status);
        Assert.Contains("invalid", f.Session.FailReason, StringComparison.OrdinalIgnoreCase);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyFailed)), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_WhenSessionAlreadyPastDiscussion_Skips()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId)
        };
        var f = Build(interviews, [proposalA], null, HirePySessionStatus.WaitingForClientApproval);

        f.Agent.Setup(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reply(90));

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        f.Agent.Verify(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_WhenRankingServiceFails_UsesRankDerivedFallback()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId),
            Interview(_roomBId, proposalB.Id, _freelancerBUserId, _freelancerBProfileId)
        };
        var f = Build(interviews, [proposalA, proposalB], SelectedCandidatesJson(proposalA, proposalB));

        f.RankingService.Setup(r => r.RankAsync(_projectId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Failure<ProjectRankingResponse>(AppError.Validation("ranking unavailable")));

        f.Agent.Setup(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()))
            .Returns((HirePyEvaluationContext ctx, CancellationToken _) =>
                Task.FromResult(Reply(ctx.CandidateName == "Dev A" ? 60 : 90)));

        var added = CaptureEvaluations(f);

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        Assert.Equal("Dev B", f.Session.SelectedCandidateName);

        var devA = added.Single(e => e.FreelancerUserId == _freelancerAUserId);
        var devB = added.Single(e => e.FreelancerUserId == _freelancerBUserId);
        Assert.Equal(100, devA.RankingScore);
        Assert.Equal(80, devB.RankingScore);
    }

    [Fact]
    public async Task GetBySessionIdAsync_ReturnsDtoOrderedByOverallScore()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var f = Build([], [], null);

        f.EvaluationRepo.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Evaluation(proposalA.Id, _freelancerAUserId, "Dev A", 55),
                Evaluation(proposalB.Id, _freelancerBUserId, "Dev B", 92)
            });

        var result = await f.Service.GetBySessionIdAsync(_sessionId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
        Assert.Equal(proposalB.Id, result.Data[0].ProposalId);
        Assert.Equal(92, result.Data[0].OverallScore);
        Assert.Contains("strength", result.Data[0].Strengths);
        Assert.Equal("Reason.", result.Data[0].Reason);
    }

    [Fact]
    public async Task GetBySessionIdAsync_WhenSessionMissing_ReturnsNotFound()
    {
        var f = Build([], [], null);
        f.SessionRepo.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HirePySession?)null);

        var result = await f.Service.GetBySessionIdAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error!.statusCode);
    }

    [Fact]
    public async Task GetBySessionIdAsync_WhenUserNotOwner_ReturnsForbidden()
    {
        var f = Build([], [], null);
        f.CurrentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var result = await f.Service.GetBySessionIdAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Error!.statusCode);
        f.EvaluationRepo.Verify(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_ResumeFromCandidateEvaluation_SkipsAiAndCompletes()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId),
            Interview(_roomBId, proposalB.Id, _freelancerBUserId, _freelancerBProfileId)
        };
        var f = Build(interviews, [proposalA, proposalB], SelectedCandidatesJson(proposalA, proposalB),
            HirePySessionStatus.CandidateEvaluation);

        f.EvaluationRepo.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Evaluation(proposalA.Id, _freelancerAUserId, "Dev A", 75),
                Evaluation(proposalB.Id, _freelancerBUserId, "Dev B", 92)
            });

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        Assert.Equal(proposalB.Id, f.Session.SelectedProposalId);
        Assert.Equal("Dev B", f.Session.SelectedCandidateName);
        f.Agent.Verify(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()), Times.Never);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateEvaluationStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyRecommendationReady)), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_ResumeFromCandidateComparison_SkipsAiAndCompletes()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId),
            Interview(_roomBId, proposalB.Id, _freelancerBUserId, _freelancerBProfileId)
        };
        var f = Build(interviews, [proposalA, proposalB], SelectedCandidatesJson(proposalA, proposalB),
            HirePySessionStatus.CandidateComparison);

        f.EvaluationRepo.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Evaluation(proposalA.Id, _freelancerAUserId, "Dev A", 75),
                Evaluation(proposalB.Id, _freelancerBUserId, "Dev B", 92)
            });

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        Assert.Equal(proposalB.Id, f.Session.SelectedProposalId);
        f.Agent.Verify(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()), Times.Never);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateComparisonStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingEvaluations_ResumeFromRecommendationReady_DoesNotRepublishRecommendation()
    {
        var proposalA = Proposal(_freelancerAUserId, 1500m);
        var proposalB = Proposal(_freelancerBUserId, 1800m);
        var interviews = new[]
        {
            Interview(_roomAId, proposalA.Id, _freelancerAUserId, _freelancerAProfileId),
            Interview(_roomBId, proposalB.Id, _freelancerBUserId, _freelancerBProfileId)
        };
        var f = Build(interviews, [proposalA, proposalB], SelectedCandidatesJson(proposalA, proposalB),
            HirePySessionStatus.RecommendationReady);

        f.EvaluationRepo.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Evaluation(proposalA.Id, _freelancerAUserId, "Dev A", 75),
                Evaluation(proposalB.Id, _freelancerBUserId, "Dev B", 92)
            });

        await f.Service.ProcessPendingEvaluationsAsync(CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        f.Agent.Verify(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()), Times.Never);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.RecommendationReady, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.WaitingForClientApproval, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static HirePyEvaluationReply Reply(int overall)
        => new()
        {
            IsValid = true,
            TechnicalScore = 70,
            RequirementsScore = 70,
            ArchitectureScore = 70,
            ImplementationScore = 70,
            MilestoneScore = overall,
            TimelineScore = 70,
            BudgetScore = 70,
            CommunicationScore = 70,
            RiskScore = 70,
            OverallScore = overall,
            Strengths = ["strength"],
            Concerns = ["concern"],
            Risks = ["risk"],
            Reason = overall >= 90 ? "Decision reason for Dev B." : "Decision reason for Dev A."
        };

    private static HirePyEvaluation Evaluation(Guid proposalId, Guid freelancerUserId, string name, int overall)
        => new()
        {
            Id = Guid.NewGuid(),
            HirePySessionId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            ProjectProposalId = proposalId,
            FreelancerUserId = freelancerUserId,
            CandidateName = name,
            OverallScore = overall,
            MilestoneScore = overall,
            StrengthsJson = "[\"strength\"]",
            ConcernsJson = "[\"concern\"]",
            RisksJson = "[\"risk\"]",
            Reason = "Reason.",
            EvaluatedAt = DateTime.UtcNow
        };

    private List<HirePyEvaluation> CaptureEvaluations(Fixture f)
    {
        var added = new List<HirePyEvaluation>();
        f.EvaluationRepo.Setup(r => r.AddAsync(It.IsAny<HirePyEvaluation>(), It.IsAny<CancellationToken>()))
            .Callback<HirePyEvaluation, CancellationToken>((e, _) => added.Add(e))
            .Returns(Task.CompletedTask);
        return added;
    }

    private string? SelectedCandidatesJson(params ProjectProposal[] proposals)
        => JsonSerializer.Serialize(proposals.Select((p, i) => new HirePySelectedCandidateDto
        {
            ProposalId = p.Id,
            CandidateName = i == 0 ? "Dev A" : "Dev B",
            Rank = i + 1,
            InviteeType = ApplicantType.User,
            InviteeId = p.UserId!.Value
        }).ToList());

    private ProjectProposal Proposal(Guid userId, decimal budget)
        => new()
        {
            Id = Guid.NewGuid(),
            ProjectId = _projectId,
            ApplicantType = ApplicantType.User,
            UserId = userId,
            CoverLetter = "I would love to build this.",
            Approach = "React + Node",
            ProposedTimeline = budget >= 1800m ? "4 weeks" : "3 weeks",
            ProposedBudget = budget,
            Status = ProposalStatus.InDiscussion,
            AppliedAt = DateTime.UtcNow
        };

    private HirePyInterview Interview(
        Guid roomId,
        Guid proposalId,
        Guid freelancerUserId,
        Guid freelancerProfileId,
        HirePyInterviewStatus status = HirePyInterviewStatus.MilestonePlanFinalized)
        => new()
        {
            Id = Guid.NewGuid(),
            HirePySessionId = _sessionId,
            ProjectId = _projectId,
            ProjectProposalId = proposalId,
            ChatRoomId = roomId,
            FreelancerUserId = freelancerUserId,
            FreelancerDeveloperProfileId = freelancerProfileId,
            Status = status,
            TurnCount = 1,
            CreatedAt = DateTime.UtcNow
        };

    private Fixture Build(
        IReadOnlyList<HirePyInterview> interviews,
        IReadOnlyList<ProjectProposal> proposals,
        string? selectedCandidatesJson,
        HirePySessionStatus sessionStatus = HirePySessionStatus.CandidateDiscussion)
    {
        var session = new HirePySession
        {
            Id = _sessionId,
            ClientUserId = _clientUserId,
            ProjectId = _projectId,
            Status = sessionStatus,
            SelectedCandidatesJson = selectedCandidatesJson,
            CreatedAt = DateTime.UtcNow
        };
        var project = new Project
        {
            Id = _projectId,
            ClientId = _clientUserId,
            Title = "Bakery Website",
            Description = "Build a bakery website.",
            Currency = "USD",
            BudgetMin = 1000,
            BudgetMax = 2000,
            Deadline = DateTime.UtcNow.AddMonths(1),
            EstimatedDurationDays = 30
        };

        var sessionRepo = new Mock<IHirePySessionRepository>();
        var interviewRepo = new Mock<IHirePyInterviewRepository>();
        var evaluationRepo = new Mock<IHirePyEvaluationRepository>();
        var projectRepo = new Mock<IProjectRepository>();
        var proposalRepo = new Mock<IProjectProposalRepository>();
        var userRepo = new Mock<IUserRepository>();
        var messageRepo = new Mock<IMessageRepository>();
        var planVersionRepo = new Mock<IMilestonePlanVersionRepository>();

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IHirePySessionRepository, HirePySession>()).Returns(sessionRepo.Object);
        unitOfWork.Setup(u => u.Repository<IHirePyInterviewRepository, HirePyInterview>()).Returns(interviewRepo.Object);
        unitOfWork.Setup(u => u.Repository<IHirePyEvaluationRepository, HirePyEvaluation>()).Returns(evaluationRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectRepository, Project>()).Returns(projectRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectProposalRepository, ProjectProposal>()).Returns(proposalRepo.Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.Repository<IMessageRepository, Message>()).Returns(messageRepo.Object);
        unitOfWork.Setup(u => u.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>()).Returns(planVersionRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        sessionRepo.Setup(r => r.GetPendingEvaluationAsync(It.IsAny<CancellationToken>())).ReturnsAsync([session]);
        sessionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(session);
        interviewRepo.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(interviews);
        evaluationRepo.Setup(r => r.GetBySessionIdAsync(_sessionId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        projectRepo.Setup(r => r.GetByIdAsync(_projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        proposalRepo.Setup(r => r.GetByProjectIdAsync(_projectId, It.IsAny<ProposalStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposals);

        userRepo.Setup(r => r.GetByIdAsync(_freelancerAUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _freelancerAUserId, FristName = "Dev", LastName = "A" });
        userRepo.Setup(r => r.GetByIdAsync(_freelancerBUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _freelancerBUserId, FristName = "Dev", LastName = "B" });
        userRepo.Setup(r => r.GetClientProfileIdByUserIdAsync(_clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_clientProfileId);

        messageRepo.Setup(r => r.GetLatestAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Message { Text = "Hi, welcome.", SenderDeveloperProfileId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            });

        planVersionRepo.Setup(r => r.GetLatestByProposalIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MilestonePlanVersion
            {
                Id = Guid.NewGuid(),
                ProjectId = _projectId,
                ProposalId = Guid.NewGuid(),
                Version = 1,
                Items =
                [
                    new MilestonePlanItem { Title = "Payment integration", Amount = 300, SortOrder = 1 }
                ]
            });

        var agent = new Mock<IHirePyEvaluatorAgent>();
        agent.Setup(a => a.EvaluateAsync(It.IsAny<HirePyEvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reply(90));

        var rankingService = new Mock<IProposalRankingService>();
        rankingService.Setup(r => r.RankAsync(_projectId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success(new ProjectRankingResponse
            {
                ProjectId = _projectId.ToString(),
                RankedProposals = proposals.Select((p, i) => new RankedProposal
                {
                    CandidateId = p.Id.ToString(),
                    CandidateName = i == 0 ? "Dev A" : "Dev B",
                    Rank = i + 1,
                    OverallScore = 95d - (i * 5d),
                    ScoreBreakdown = new ScoreBreakdown()
                }).ToList(),
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = proposals.Count,
                    ReturnedCount = proposals.Count,
                    ProcessingTime = TimeSpan.Zero
                }
            }));

        var eventPublisher = new Mock<IHirePyEventPublisher>();
        var notificationService = new Mock<INotificationService>();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(_clientUserId);

        var service = new HirePyEvaluationService(
            unitOfWork.Object,
            agent.Object,
            rankingService.Object,
            eventPublisher.Object,
            notificationService.Object,
            currentUser.Object);

        return new Fixture(
            service, session, sessionRepo, interviewRepo, evaluationRepo, projectRepo, proposalRepo,
            userRepo, messageRepo, planVersionRepo, agent, rankingService, eventPublisher,
            notificationService, currentUser);
    }
}
