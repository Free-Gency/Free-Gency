using FreeGency.AI.Ranking.ProposalRanking;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.HirePy;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.ProjectInvitations.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FreeGency.Tests;

public class HirePyCandidateServiceTests
{
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();

    private sealed record Fixture(
        HirePyCandidateService Service,
        HirePySession Session,
        Mock<IProjectRepository> ProjectRepo,
        Mock<IProjectService> ProjectService,
        Mock<IProjectProposalRepository> ProposalRepo,
        Mock<IProjectInvitationRepository> InvitationRepo,
        Mock<IProposalRankingService> RankingService,
        Mock<IUserRepository> UserRepo,
        Mock<IDeveloperProfileRepository> DeveloperProfileRepo,
        Mock<ITeamRepository> TeamRepo,
        Mock<IProjectInvitationService> InvitationService,
        Mock<IHirePyEventPublisher> EventPublisher,
        Mock<INotificationService> NotificationService,
        Mock<IHirePyInterviewService> InterviewService);

    [Fact]
    public async Task ProcessRankingAndInvitations_Success_RanksInvitesAndMovesToWaiting()
    {
        var f = Build();

        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: Guid.NewGuid()),
            Proposal(ApplicantType.User, userId: Guid.NewGuid()),
            Proposal(ApplicantType.User, userId: Guid.NewGuid()),
        };
        var ranked = proposals.Select((p, i) => Ranked(p.Id, $"Dev {i + 1}", i + 1)).ToList();

        SetupRanking(f, proposals, ranked);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        Assert.Contains("ProposalId", f.Session.SelectedCandidatesJson);

        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.IsAny<CreateProjectInvitationDto>(), _clientUserId, It.IsAny<CancellationToken>()), Times.Exactly(3));

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.RankingStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.RankingCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.TopCandidatesSelected, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.InvitationsSent, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyInvitationsSent)), Times.Once);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_WithTenRanked_InvitesOnlyTopFive()
    {
        var f = Build();

        var proposals = Enumerable.Range(0, 10)
            .Select(_ => Proposal(ApplicantType.User, userId: Guid.NewGuid()))
            .ToList();
        var ranked = proposals.Select((p, i) => Ranked(p.Id, $"Dev {i + 1}", i + 1)).ToList();

        SetupRanking(f, proposals, ranked);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.IsAny<CreateProjectInvitationDto>(), _clientUserId, It.IsAny<CancellationToken>()), Times.Exactly(5));
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_WithZeroCandidates_MarksFailedAndNotifies()
    {
        var f = Build();

        SetupRanking(f, [], []);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, f.Session.Status);
        Assert.Contains("No eligible candidates", f.Session.FailReason);

        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.RankingFailed, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyNoCandidatesFound)), Times.Once);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_WhenProjectIsDraft_PublishesBeforeRanking()
    {
        var f = Build();

        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: Guid.NewGuid()),
        };
        SetupRanking(f, proposals, [Ranked(proposals[0].Id, "Dev", 1)], projectStatus: ProjectStatus.Draft);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.ProjectService.Verify(p => p.PublishForClientAsync(_projectId, _clientUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_SkipsAlreadyInvitedCandidates()
    {
        var f = Build();

        var invitedUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: invitedUserId),
            Proposal(ApplicantType.User, userId: otherUserId),
        };
        var ranked = proposals.Select((p, i) => Ranked(p.Id, $"Dev {i + 1}", i + 1)).ToList();
        var existing = new[]
        {
            new ProjectInvitation
            {
                Id = Guid.NewGuid(),
                ProjectId = _projectId,
                InviteeType = ApplicantType.User,
                InviteeUserId = invitedUserId,
                Status = ProjectInvitationStatus.Pending
            }
        };

        SetupRanking(f, proposals, ranked, existingInvitations: existing);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.IsAny<CreateProjectInvitationDto>(), _clientUserId, It.IsAny<CancellationToken>()), Times.Once);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == invitedUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_SkipsAlreadyHiredAndInvalidProposals()
    {
        var f = Build();

        var hiredUserId = Guid.NewGuid();
        var rejectedUserId = Guid.NewGuid();
        var eligibleUserId = Guid.NewGuid();
        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: hiredUserId),
            Proposal(ApplicantType.User, userId: rejectedUserId, status: ProposalStatus.Rejected),
            Proposal(ApplicantType.User, userId: eligibleUserId),
        };
        var ranked = proposals.Select((p, i) => Ranked(p.Id, $"Dev {i + 1}", i + 1)).ToList();

        SetupRanking(f, proposals, ranked);
        f.ProjectRepo.Setup(r => r.GetByIdAsync(_projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project
            {
                Id = _projectId,
                ClientId = _clientUserId,
                Title = "Bakery Website",
                Status = ProjectStatus.Open,
                AssignedUserId = hiredUserId
            });

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == hiredUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Never);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == rejectedUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Never);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == eligibleUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_DeduplicatesDuplicateProposals()
    {
        var f = Build();

        var sameUserId = Guid.NewGuid();
        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: sameUserId),
            Proposal(ApplicantType.User, userId: sameUserId),
            Proposal(ApplicantType.User, userId: Guid.NewGuid()),
        };
        var ranked = proposals.Select((p, i) => Ranked(p.Id, $"Dev {i + 1}", i + 1)).ToList();

        SetupRanking(f, proposals, ranked);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.IsAny<CreateProjectInvitationDto>(), _clientUserId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == sameUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_SkipsInactiveFreelancers()
    {
        var f = Build();

        var inactiveUserId = Guid.NewGuid();
        var activeUserId = Guid.NewGuid();
        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: inactiveUserId),
            Proposal(ApplicantType.User, userId: activeUserId),
        };
        var ranked = proposals.Select((p, i) => Ranked(p.Id, $"Dev {i + 1}", i + 1)).ToList();

        SetupRanking(f, proposals, ranked,
            users: new[]
            {
                new User { Id = inactiveUserId, IsDeleted = true },
                new User { Id = activeUserId, IsDeleted = false },
            },
            profiles: new[]
            {
                new DeveloperProfile { UserId = activeUserId, IsDeleted = false },
            });

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == inactiveUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Never);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.Is<CreateProjectInvitationDto>(d => d.InviteeUserId == activeUserId),
            _clientUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRankingAndInvitations_RetryAfterSuccess_DoesNotSendDuplicateInvitations()
    {
        var f = Build();

        var proposals = new[]
        {
            Proposal(ApplicantType.User, userId: Guid.NewGuid()),
        };
        SetupRanking(f, proposals, [Ranked(proposals[0].Id, "Dev", 1)]);

        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);
        await f.Service.ProcessRankingAndInvitationsAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, f.Session.Status);
        f.InvitationService.Verify(i => i.CreateForClientAsync(
            It.IsAny<CreateProjectInvitationDto>(), _clientUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnInvitationAccepted_MovesSessionToCandidateDiscussion_PublishesEvent_AndStartsInterview()
    {
        var session = Session(HirePySessionStatus.WaitingForCandidates);
        var f = Build(session);
        var proposalId = Guid.NewGuid();
        var freelancerUserId = Guid.NewGuid();

        await f.Service.OnInvitationAcceptedAsync(_projectId, proposalId, freelancerUserId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.CandidateDiscussion, session.Status);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateAccepted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.InterviewService.Verify(s => s.EnsureInterviewAsync(
            _sessionId, proposalId, freelancerUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnInvitationDeclined_PublishesEventAndKeepsWaiting()
    {
        var session = Session(HirePySessionStatus.WaitingForCandidates);
        var f = Build(session);

        await f.Service.OnInvitationDeclinedAsync(_projectId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.WaitingForCandidates, session.Status);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.CandidateDeclined, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private HirePySession Session(HirePySessionStatus status = HirePySessionStatus.ProjectCreated)
        => new()
        {
            Id = _sessionId,
            ClientUserId = _clientUserId,
            ProjectId = _projectId,
            Description = "I need a bakery website",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

    private ProjectProposal Proposal(
        ApplicantType type,
        Guid? userId = null,
        Guid? teamId = null,
        ProposalStatus status = ProposalStatus.Pending)
        => new()
        {
            Id = Guid.NewGuid(),
            ProjectId = _projectId,
            ApplicantType = type,
            UserId = type == ApplicantType.User ? userId : null,
            TeamId = type == ApplicantType.Team ? teamId : null,
            Status = status,
            AppliedAt = DateTime.UtcNow
        };

    private static RankedProposal Ranked(Guid proposalId, string name, int rank)
        => new()
        {
            CandidateId = proposalId.ToString(),
            CandidateName = name,
            Rank = rank,
            OverallScore = 100d - rank,
            ScoreBreakdown = new ScoreBreakdown()
        };

    private Fixture Build(HirePySession? session = null)
    {
        var sessionRepo = new Mock<IHirePySessionRepository>();
        var projectRepo = new Mock<IProjectRepository>();
        var proposalRepo = new Mock<IProjectProposalRepository>();
        var invitationRepo = new Mock<IProjectInvitationRepository>();
        var userRepo = new Mock<IUserRepository>();
        var developerProfileRepo = new Mock<IDeveloperProfileRepository>();
        var teamRepo = new Mock<ITeamRepository>();

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IHirePySessionRepository, HirePySession>()).Returns(sessionRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectRepository, Project>()).Returns(projectRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectProposalRepository, ProjectProposal>()).Returns(proposalRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectInvitationRepository, ProjectInvitation>()).Returns(invitationRepo.Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.Repository<IDeveloperProfileRepository, DeveloperProfile>()).Returns(developerProfileRepo.Object);
        unitOfWork.Setup(u => u.Repository<ITeamRepository, Team>()).Returns(teamRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var rankingService = new Mock<IProposalRankingService>();
        var invitationService = new Mock<IProjectInvitationService>();
        var projectService = new Mock<IProjectService>();
        var eventPublisher = new Mock<IHirePyEventPublisher>();
        var notificationService = new Mock<INotificationService>();
        var interviewService = new Mock<IHirePyInterviewService>();

        userRepo.Setup(r => r.GetClientProfileIdByUserIdAsync(_clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_clientProfileId);

        var service = new HirePyCandidateService(
            unitOfWork.Object,
            rankingService.Object,
            invitationService.Object,
            projectService.Object,
            eventPublisher.Object,
            notificationService.Object,
            interviewService.Object);

        var theSession = session ?? Session();
        sessionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(theSession);
        sessionRepo.Setup(r => r.GetByProjectIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(theSession);

        return new Fixture(
            service, theSession, projectRepo, projectService, proposalRepo, invitationRepo,
            rankingService, userRepo, developerProfileRepo, teamRepo, invitationService,
            eventPublisher, notificationService, interviewService);
    }

    private void SetupRanking(
        Fixture f,
        IReadOnlyList<ProjectProposal> proposals,
        IReadOnlyList<RankedProposal> ranked,
        ProjectStatus projectStatus = ProjectStatus.Open,
        IReadOnlyList<ProjectInvitation>? existingInvitations = null,
        IReadOnlyList<User>? users = null,
        IReadOnlyList<DeveloperProfile>? profiles = null,
        IReadOnlyList<Team>? teams = null)
    {
        f.ProjectRepo.Setup(r => r.GetByIdAsync(_projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project { Id = _projectId, ClientId = _clientUserId, Title = "Bakery Website", Status = projectStatus });
        f.ProposalRepo.Setup(r => r.GetByProjectIdAsync(_projectId, It.IsAny<ProposalStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposals);
        f.RankingService.Setup(r => r.RankAsync(_projectId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success(new ProjectRankingResponse
            {
                ProjectId = _projectId.ToString(),
                RankedProposals = ranked,
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = ranked.Count,
                    ReturnedCount = ranked.Count,
                    ProcessingTime = TimeSpan.Zero
                }
            }));
        f.InvitationRepo.Setup(r => r.GetByProjectIdAsync(_projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvitations ?? []);

        f.InvitationService.Setup(i => i.CreateForClientAsync(
                It.IsAny<CreateProjectInvitationDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success(new ProjectInvitationDto { Id = Guid.NewGuid() }));

        var userList = (users ?? proposals
                .Where(p => p.ApplicantType == ApplicantType.User && p.UserId.HasValue)
                .Select(p => new User { Id = p.UserId!.Value, IsDeleted = false })
                .DistinctBy(u => u.Id))
            .ToList();
        var teamList = (teams ?? proposals
                .Where(p => p.ApplicantType == ApplicantType.Team && p.TeamId.HasValue)
                .Select(p => new Team { Id = p.TeamId!.Value, IsDeleted = false })
                .DistinctBy(t => t.Id))
            .ToList();
        var profileList = (profiles ?? userList
                .Select(u => new DeveloperProfile { UserId = u.Id, IsDeleted = false }))
            .ToList();

        f.UserRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<User>(userList));
        f.DeveloperProfileRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<DeveloperProfile>(profileList));
        f.TeamRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Team>(teamList));

        f.ProjectService.Setup(p => p.PublishForClientAsync(_projectId, _clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success("ok"));
    }
}
