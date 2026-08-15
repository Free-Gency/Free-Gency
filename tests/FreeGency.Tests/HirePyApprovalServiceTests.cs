using System.Text.Json;
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

public class HirePyApprovalServiceTests
{
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _freelancerUserId = Guid.NewGuid();
    private readonly Guid _proposalId = Guid.NewGuid();
    private readonly Guid _skillId = Guid.NewGuid();
    private readonly Guid _planId = Guid.NewGuid();

    private sealed record Fixture(
        HirePyApprovalService Service,
        HirePySession Session,
        Project Project,
        ProjectProposal Proposal,
        MilestonePlanVersion Plan,
        Mock<IHirePySessionRepository> SessionRepo,
        Mock<IProjectRepository> ProjectRepo,
        Mock<IProjectProposalRepository> ProposalRepo,
        Mock<IHirePyEvaluationRepository> EvaluationRepo,
        Mock<IMilestonePlanVersionRepository> PlanVersionRepo,
        Mock<ISkillRepository> SkillRepo,
        Mock<IUserRepository> UserRepo,
        Mock<IMilestoneService> MilestoneService,
        Mock<IHirePyEventPublisher> EventPublisher,
        Mock<INotificationService> NotificationService);

    [Fact]
    public async Task GetRecommendation_ReturnsClientSafeSummary()
    {
        var f = Build();

        var result = await f.Service.GetRecommendationAsync(_sessionId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_sessionId, result.Data!.SessionId);
        Assert.Equal(_projectId, result.Data.ProjectId);

        Assert.Equal("Bakery Website", result.Data.Project.Title);
        Assert.Equal("Build a bakery website.", result.Data.Project.Description);
        Assert.Contains("Menu display", result.Data.Project.Requirements);
        Assert.Contains("React", result.Data.Project.Skills);
        Assert.Equal(1000m, result.Data.Project.BudgetMin);
        Assert.Equal(2000m, result.Data.Project.BudgetMax);
        Assert.NotNull(result.Data.Project.Deadline);

        Assert.Equal(_freelancerUserId, result.Data.Freelancer.FreelancerUserId);
        Assert.Equal("Dev A", result.Data.Freelancer.CandidateName);
        Assert.Equal(1, result.Data.Freelancer.RankingPosition);
        Assert.Equal(95, result.Data.Freelancer.RankingScore);

        Assert.Equal(_proposalId, result.Data.Proposal.ProposalId);
        Assert.Equal("React + Node", result.Data.Proposal.Approach);
        Assert.Equal(1500m, result.Data.Proposal.ProposedBudget);
        Assert.Equal("3 weeks", result.Data.Proposal.ProposedTimeline);

        Assert.Equal(92, result.Data.Evaluation.OverallScore);
        Assert.Contains("strength", result.Data.Evaluation.Strengths);
        Assert.Contains("concern", result.Data.Evaluation.Concerns);
        Assert.Contains("risk", result.Data.Evaluation.Risks);
        Assert.Equal("Great fit.", result.Data.Evaluation.Reason);
        Assert.Contains("Payment integration", result.Data.Evaluation.MilestoneSummary);
        Assert.Contains("Within the USD", result.Data.Evaluation.BudgetCompatibility);
    }

    [Fact]
    public async Task GetRecommendation_ForNonOwner_ReturnsForbidden()
    {
        var f = Build(userId: Guid.NewGuid());

        var result = await f.Service.GetRecommendationAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Error!.statusCode);
    }

    [Fact]
    public async Task GetRecommendation_WhenSessionMissing_ReturnsNotFound()
    {
        var f = Build();
        f.SessionRepo.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HirePySession?)null);

        var result = await f.Service.GetRecommendationAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error!.statusCode);
    }

    [Fact]
    public async Task GetRecommendation_WhenNoRecommendation_ReturnsValidation()
    {
        var f = Build();
        f.Session.SelectedProposalId = null;
        f.Session.SelectedFreelancerUserId = null;

        var result = await f.Service.GetRecommendationAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error!.statusCode);
    }

    [Fact]
    public async Task ApproveAndHire_Success_HiresAndCompletes()
    {
        var f = Build();

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.True(result.IsSuccess);

        Assert.Equal(HirePySessionStatus.Completed, f.Session.Status);
        Assert.NotNull(f.Session.CompletedAt);
        Assert.Equal(_planId, f.Session.AcceptedPlanVersionId);

        f.SessionRepo.Verify(r => r.TryTransitionStatusAsync(
            _sessionId,
            HirePySessionStatus.WaitingForClientApproval,
            HirePySessionStatus.Hiring,
            It.IsAny<CancellationToken>()), Times.Once);
        f.SessionRepo.Verify(r => r.TryTransitionStatusAsync(
            _sessionId,
            HirePySessionStatus.Hiring,
            HirePySessionStatus.Completed,
            It.IsAny<CancellationToken>()), Times.Once);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(_planId, It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HiringStarted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HiringCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HirePyCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyHiringCompleted)), Times.Once);
    }

    [Fact]
    public async Task ApproveAndHire_ForNonOwner_ReturnsForbidden()
    {
        var f = Build(userId: Guid.NewGuid());

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Error!.statusCode);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenNoRecommendation_ReturnsValidation()
    {
        var f = Build();
        f.Session.SelectedProposalId = null;
        f.Session.SelectedFreelancerUserId = null;
        f.Session.RecommendationCompletedAt = null;

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error!.statusCode);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenCandidateNotEligible_ReturnsConflict()
    {
        var f = Build();
        f.Proposal.Status = ProposalStatus.Rejected;

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        Assert.Contains("eligible", result.Error!.message, StringComparison.OrdinalIgnoreCase);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenProjectNotAvailable_ReturnsConflict()
    {
        var f = Build();
        f.Project.AssignedUserId = Guid.NewGuid();

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        Assert.Contains("available", result.Error!.message, StringComparison.OrdinalIgnoreCase);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenPlanNotProposed_ReturnsConflict()
    {
        var f = Build();
        f.Plan.Status = PlanVersionStatus.Accepted;

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenAlreadyCompleted_ReturnsConflict()
    {
        var f = Build();
        f.Session.Status = HirePySessionStatus.Completed;

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenHiring_ResumesAndCompletesWithoutReAccepting()
    {
        var f = Build();
        f.Session.Status = HirePySessionStatus.Hiring;
        f.Plan.Status = PlanVersionStatus.Accepted;

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(HirePySessionStatus.Completed, f.Session.Status);
        Assert.NotNull(f.Session.CompletedAt);
        Assert.Equal(_planId, f.Session.AcceptedPlanVersionId);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        f.SessionRepo.Verify(r => r.TryTransitionStatusAsync(
            _sessionId,
            HirePySessionStatus.Hiring,
            HirePySessionStatus.Completed,
            It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HiringCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        f.NotificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r => r.Type == NotificationType.HirePyHiringCompleted)), Times.Once);
    }

    [Fact]
    public async Task ApproveAndHire_WhenHiringResumeLosesCompletionRace_ReturnsSuccessWithoutPublishing()
    {
        var f = Build();
        f.Session.Status = HirePySessionStatus.Hiring;
        f.Plan.Status = PlanVersionStatus.Accepted;
        f.SessionRepo.Setup(r => r.TryTransitionStatusAsync(
                _sessionId,
                HirePySessionStatus.Hiring,
                HirePySessionStatus.Completed,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HiringCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        f.NotificationService.Verify(n => n.CreateNotification(It.IsAny<CreateNotificationRequest>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenProposalNotInProject_ReturnsConflict()
    {
        var f = Build();
        f.Proposal.ProjectId = Guid.NewGuid();

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        Assert.Contains("project", result.Error!.message, StringComparison.OrdinalIgnoreCase);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenFreelancerMissing_ReturnsConflict()
    {
        var f = Build();
        f.UserRepo.Setup(r => r.GetByIdAsync(_freelancerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        Assert.Contains("freelancer", result.Error!.message, StringComparison.OrdinalIgnoreCase);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenConcurrentClaimLost_ReturnsConflictAndDoesNotHire()
    {
        var f = Build();
        f.SessionRepo.Setup(r => r.TryTransitionStatusAsync(
                _sessionId,
                HirePySessionStatus.WaitingForClientApproval,
                HirePySessionStatus.Hiring,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Error!.statusCode);
        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        f.MilestoneService.Verify(m => m.AcceptPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HiringCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAndHire_WhenHiringServiceFails_RestoresAwaitingApproval()
    {
        var f = Build();
        f.MilestoneService.Setup(m => m.AcceptPlanAsync(_planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Failure(FreeGency.Application.Common.Errors.AppError.Validation("hire failed")));

        var result = await f.Service.ApproveAndHireAsync(_sessionId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(HirePySessionStatus.WaitingForClientApproval, f.Session.Status);
        f.SessionRepo.Verify(r => r.TryTransitionStatusAsync(
            _sessionId,
            HirePySessionStatus.Hiring,
            HirePySessionStatus.WaitingForClientApproval,
            It.IsAny<CancellationToken>()), Times.Once);
        f.EventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId, HirePyEventNames.HiringCompleted, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        f.NotificationService.Verify(n => n.CreateNotification(It.IsAny<CreateNotificationRequest>()), Times.Never);
    }

    private Fixture Build(Guid? userId = null)
    {
        var session = new HirePySession
        {
            Id = _sessionId,
            ClientUserId = _clientUserId,
            ProjectId = _projectId,
            Status = HirePySessionStatus.WaitingForClientApproval,
            Title = "Bakery Website",
            CategoryName = "Software Development",
            RequirementsJson = JsonSerializer.Serialize(new[] { "Order form", "Menu display" }),
            SkillIdsJson = JsonSerializer.Serialize(new[] { _skillId }),
            SelectedProposalId = _proposalId,
            SelectedFreelancerUserId = _freelancerUserId,
            SelectedCandidateName = "Dev A",
            DecisionReason = "Great fit.",
            MilestoneSummary = "1. Payment integration",
            SelectedBudget = 1500m,
            SelectedTimeline = "3 weeks",
            RecommendationCompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var project = new Project
        {
            Id = _projectId,
            ClientId = _clientUserId,
            Title = "Bakery Website",
            Description = "Build a bakery website.",
            BudgetMin = 1000,
            BudgetMax = 2000,
            Currency = "USD",
            Deadline = DateTime.UtcNow.AddMonths(1),
            EstimatedDurationDays = 30,
            Status = ProjectStatus.Open
        };

        var proposal = new ProjectProposal
        {
            Id = _proposalId,
            ProjectId = _projectId,
            ApplicantType = ApplicantType.User,
            UserId = _freelancerUserId,
            CoverLetter = "I would love to build this.",
            Approach = "React + Node",
            ProposedTimeline = "3 weeks",
            ProposedBudget = 1500m,
            Status = ProposalStatus.InDiscussion,
            AppliedAt = DateTime.UtcNow
        };

        var plan = new MilestonePlanVersion
        {
            Id = _planId,
            ProjectId = _projectId,
            ProposalId = _proposalId,
            Version = 1,
            Status = PlanVersionStatus.Proposed,
            ProposedByUserId = _freelancerUserId,
            Items =
            [
                new MilestonePlanItem { Title = "Payment integration", Amount = 1500, SortOrder = 1 }
            ]
        };

        var evaluation = new HirePyEvaluation
        {
            Id = Guid.NewGuid(),
            HirePySessionId = _sessionId,
            ProjectId = _projectId,
            ProjectProposalId = _proposalId,
            FreelancerUserId = _freelancerUserId,
            CandidateName = "Dev A",
            RankingPosition = 1,
            RankingScore = 95,
            OverallScore = 92,
            MilestoneScore = 90,
            StrengthsJson = "[\"strength\"]",
            ConcernsJson = "[\"concern\"]",
            RisksJson = "[\"risk\"]",
            Reason = "Great fit.",
            MilestoneSummary = "1. Payment integration",
            EvaluatedAt = DateTime.UtcNow
        };

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(userId ?? _clientUserId);

        var sessionRepo = new Mock<IHirePySessionRepository>();
        sessionRepo.Setup(r => r.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        sessionRepo.Setup(r => r.TryTransitionStatusAsync(
                It.IsAny<Guid>(), It.IsAny<HirePySessionStatus>(), It.IsAny<HirePySessionStatus>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.GetByIdAsync(_projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var proposalRepo = new Mock<IProjectProposalRepository>();
        proposalRepo.Setup(r => r.GetByIdAsync(_proposalId, It.IsAny<CancellationToken>())).ReturnsAsync(proposal);

        var evaluationRepo = new Mock<IHirePyEvaluationRepository>();
        evaluationRepo.Setup(r => r.GetByProposalIdAsync(_proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([evaluation]);

        var planVersionRepo = new Mock<IMilestonePlanVersionRepository>();
        planVersionRepo.Setup(r => r.GetLatestByProposalIdAsync(_proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var skillRepo = new Mock<ISkillRepository>();
        skillRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Skill { Id = _skillId, Name = "React" }]);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetClientProfileIdByUserIdAsync(_clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_clientProfileId);
        userRepo.Setup(r => r.GetByIdAsync(_freelancerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _freelancerUserId });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IHirePySessionRepository, HirePySession>()).Returns(sessionRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectRepository, Project>()).Returns(projectRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectProposalRepository, ProjectProposal>()).Returns(proposalRepo.Object);
        unitOfWork.Setup(u => u.Repository<IHirePyEvaluationRepository, HirePyEvaluation>()).Returns(evaluationRepo.Object);
        unitOfWork.Setup(u => u.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>()).Returns(planVersionRepo.Object);
        unitOfWork.Setup(u => u.Repository<ISkillRepository, Skill>()).Returns(skillRepo.Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var milestoneService = new Mock<IMilestoneService>();
        milestoneService.Setup(m => m.AcceptPlanAsync(_planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success("hire complete"));

        var eventPublisher = new Mock<IHirePyEventPublisher>();
        var notificationService = new Mock<INotificationService>();

        var service = new HirePyApprovalService(
            currentUser.Object,
            unitOfWork.Object,
            milestoneService.Object,
            eventPublisher.Object,
            notificationService.Object);

        return new Fixture(
            service, session, project, proposal, plan,
            sessionRepo, projectRepo, proposalRepo, evaluationRepo, planVersionRepo,
            skillRepo, userRepo, milestoneService, eventPublisher, notificationService);
    }
}
