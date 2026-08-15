using System.Text.Json;
using FreeGency.AI.ProjectDrafting;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.HirePy;
using FreeGency.Application.Features.HirePy.Dtos;
using FreeGency.Application.Features.Moderation.DTOs;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.Projects.DTOs;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FluentValidation;
using FreeGency.Infrastructure.Interfaces;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FreeGency.Tests;

public class HirePySessionServiceTests
{
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();

    private static readonly GeneratedProjectDraft ValidDraft = new()
    {
        Title = "Bakery Website",
        Description = "A polished marketing site for a bakery.",
        CategoryId = Guid.NewGuid(),
        CategoryName = "Software Development",
        SpecialtyIds = [Guid.NewGuid()],
        SkillIds = [Guid.NewGuid()],
        IsFixedPrice = true,
        BudgetMin = 500m,
        BudgetMax = 800m,
        Currency = "USD",
        EstimatedDurationDays = 30,
        Deadline = new DateTime(2026, 12, 31),
        Complexity = "Medium",
        Requirements = ["Order form", "Menu display"],
        Features = ["Gallery"],
        Risks = ["Late assets"],
    };

    private (HirePySessionService service,
             Mock<IHirePySessionRepository> sessionRepo,
             Mock<IUserRepository> userRepo,
             Mock<IProjectService> projectService,
             Mock<IProjectGenerationService> generationService,
             Mock<INotificationService> notificationService,
             Mock<IHirePyEventPublisher> eventPublisher,
             Mock<IBackgroundJobClient> backgroundJobClient,
             Mock<IUnitOfWork> unitOfWork,
             Mock<ICurrentUserService> currentUser,
             Mock<IProjectRepository> projectRepo,
             HirePySession session) Build(Guid userId, HirePySession? session = null, Mock<IContentModerationService>? moderation = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(userId);

        var sessionRepo = new Mock<IHirePySessionRepository>();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetClientProfileIdByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_clientProfileId);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IHirePySessionRepository, HirePySession>()).Returns(sessionRepo.Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.GetProjectsQuery()).Returns(Array.Empty<Project>().AsQueryable());
        unitOfWork.Setup(u => u.Repository<IProjectRepository, Project>()).Returns(projectRepo.Object);

        var projectService = new Mock<IProjectService>();
        var generationService = new Mock<IProjectGenerationService>();
        var notificationService = new Mock<INotificationService>();
        var eventPublisher = new Mock<IHirePyEventPublisher>();
        var backgroundJobClient = new Mock<IBackgroundJobClient>();
        var contentModerationService = moderation ?? new Mock<IContentModerationService>();
        if (moderation is null)
        {
            contentModerationService.Setup(m => m.ModerateAndEnforceAsync(
                    It.IsAny<Guid>(), It.IsAny<ModerationSourceType>(), It.IsAny<Guid>(), It.IsAny<string?>(),
                    It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentModerationResult
                {
                    Action = ModerationAction.Allow,
                    Status = ModerationStatus.Visible,
                    SafeText = null
                });
        }

        var service = new HirePySessionService(
            currentUser.Object,
            unitOfWork.Object,
            projectService.Object,
            generationService.Object,
            notificationService.Object,
            eventPublisher.Object,
            backgroundJobClient.Object,
            new CreateProjectValidator(),
            contentModerationService.Object);

        var theSession = session ?? new HirePySession
        {
            Id = _sessionId,
            ClientUserId = _clientUserId,
            Description = "I need a bakery website",
            Status = HirePySessionStatus.GeneratingProject,
            CreatedAt = DateTime.UtcNow
        };
        sessionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(theSession);

        return (service, sessionRepo, userRepo, projectService, generationService,
            notificationService, eventPublisher, backgroundJobClient, unitOfWork, currentUser, projectRepo, theSession);
    }

    [Fact]
    public async Task StartAsync_WithValidDescription_CreatesSessionAndEnqueuesJob()
    {
        var (service, sessionRepo, _, _, _, _, eventPublisher, backgroundJobClient, _, _, _, _) =
            Build(_clientUserId);

        HirePySession? added = null;
        sessionRepo.Setup(r => r.AddAsync(It.IsAny<HirePySession>(), It.IsAny<CancellationToken>()))
            .Callback<HirePySession, CancellationToken>((s, _) => added = s)
            .Returns(Task.CompletedTask);

        var response = await service.StartAsync(new StartHirePySessionRequestDto
        {
            Description = "I need a bakery website"
        }, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(added!.Id, response.Data.SessionId);
        Assert.Equal("Processing", response.Data.Status);
        Assert.Equal(nameof(HirePySessionStatus.GeneratingProject), response.Data.Stage);
        Assert.Equal(_clientUserId, added.ClientUserId);
        Assert.Equal(HirePySessionStatus.GeneratingProject, added.Status);

        eventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId,
            HirePyEventNames.Started,
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);

        backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithBlankDescription_ReturnsValidationError()
    {
        var (service, _, _, _, _, _, _, _, _, _, _, _) = Build(_clientUserId);

        var response = await service.StartAsync(new StartHirePySessionRequestDto
        {
            Description = "   "
        }, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.Error!.statusCode);
    }

    [Fact]
    public async Task StartAsync_WhenClientNotAuthenticated_ReturnsUnauthorized()
    {
        var (service, _, _, _, _, _, _, _, _, _, _, _) = Build(Guid.Empty);

        var response = await service.StartAsync(new StartHirePySessionRequestDto
        {
            Description = "I need a bakery website"
        }, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status401Unauthorized, response.Error!.statusCode);
    }

    [Fact]
    public async Task GetAsync_ReturnsSessionForOwner()
    {
        var (service, _, _, _, _, _, _, _, _, _, _, session) = Build(_clientUserId);

        var response = await service.GetAsync(_sessionId, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(_sessionId, response.Data.Id);
        Assert.Equal("Processing", response.Data.Status);
        Assert.Equal(nameof(HirePySessionStatus.GeneratingProject), response.Data.Stage);
    }

    [Fact]
    public async Task GetAsync_ForNonOwner_ReturnsForbidden()
    {
        var (service, _, _, _, _, _, _, _, _, _, _, _) = Build(Guid.NewGuid());

        var response = await service.GetAsync(_sessionId, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status403Forbidden, response.Error!.statusCode);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_Success_CreatesProjectAndNotifies()
    {
        var (service, _, _, projectService, generationService, notificationService, eventPublisher, _, _, _, _, session) =
            Build(_clientUserId);

        generationService.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidDraft);
        projectService.Setup(p => p.CreateForClientAsync(It.IsAny<CreateProjectRequestDto>(), _clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success(_projectId));

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.ProjectCreated, session.Status);
        Assert.Equal(_projectId, session.ProjectId);
        Assert.Equal(ValidDraft.Title, session.Title);
        Assert.Equal(500m, session.BudgetMin);

        projectService.Verify(p => p.CreateForClientAsync(
            It.Is<CreateProjectRequestDto>(d =>
                d.CategoryId == ValidDraft.CategoryId && d.Currency == "USD"),
            _clientUserId,
            It.IsAny<CancellationToken>()), Times.Once);

        eventPublisher.Verify(p => p.PublishClientEventAsync(
            _clientUserId,
            HirePyEventNames.ProjectCreated,
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);

        notificationService.Verify(n => n.CreateNotification(
            It.Is<CreateNotificationRequest>(r =>
                r.Type == NotificationType.HirePyProjectCreated && r.ClientProfileId == _clientProfileId)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_WhenAiFails_MarksSessionFailed()
    {
        var (service, _, _, _, generationService, _, _, _, _, _, _, session) = Build(_clientUserId);

        generationService.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Bedrock gateway unreachable"));

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, session.Status);
        Assert.Contains("AI project generation failed", session.FailReason);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_WhenAiReturnsMalformedResponse_MarksSessionFailed()
    {
        var (service, _, _, _, generationService, _, _, _, _, _, _, session) = Build(_clientUserId);

        generationService.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("The AI returned a malformed project plan."));

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, session.Status);
        Assert.Contains("AI project generation failed", session.FailReason);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_WhenCategoryCannotBeMapped_MarksSessionFailed()
    {
        var (service, _, _, _, generationService, _, _, _, _, _, _, session) = Build(_clientUserId);

        generationService.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedProjectDraft
            {
                Title = "Vague idea",
                Description = "Something.",
                NeedsManualCategoryReview = true,
                SkillIds = [],
            });

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, session.Status);
        Assert.Contains("supported category", session.FailReason);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_WhenAiOutputFailsProjectValidation_MarksSessionFailed()
    {
        var (service, _, _, _, generationService, _, _, _, _, _, _, session) = Build(_clientUserId);

        var invalidDraft = new GeneratedProjectDraft
        {
            Title = "Bakery Website",
            Description = "A polished marketing site.",
            CategoryId = Guid.NewGuid(),
            SpecialtyIds = [Guid.NewGuid()],
            SkillIds = [Guid.NewGuid()],
            BudgetMin = 800m,
            BudgetMax = 100m,
            Currency = "USD",
        };
        generationService.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidDraft);

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, session.Status);
        Assert.Contains("normal project validation", session.FailReason);
        Assert.Null(session.ProjectId);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_WhenProjectCreationFails_MarksSessionFailed()
    {
        var (service, _, _, projectService, generationService, _, _, _, _, _, _, session) = Build(_clientUserId);

        generationService.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidDraft);
        projectService.Setup(p => p.CreateForClientAsync(It.IsAny<CreateProjectRequestDto>(), _clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Failure<Guid>(AppError.Validation("Category does not exist.")));

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(HirePySessionStatus.Failed, session.Status);
        Assert.Contains("Project creation failed", session.FailReason);
        Assert.Null(session.ProjectId);
    }

    [Fact]
    public async Task StartAsync_WhenDescriptionBlockedByModeration_ReturnsValidationError()
    {
        var moderation = new Mock<IContentModerationService>();
        moderation.Setup(m => m.ModerateAndEnforceAsync(
                It.IsAny<Guid>(), It.IsAny<ModerationSourceType>(), It.IsAny<Guid>(), It.IsAny<string?>(),
                It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentModerationResult
            {
                Action = ModerationAction.Hide,
                Status = ModerationStatus.Hidden,
                WarningMessage = "Contact details not allowed"
            });
        var (service, sessionRepo, _, _, _, _, _, _, _, _, _, _) = Build(_clientUserId, moderation: moderation);

        HirePySession? added = null;
        sessionRepo.Setup(r => r.AddAsync(It.IsAny<HirePySession>(), It.IsAny<CancellationToken>()))
            .Callback<HirePySession, CancellationToken>((s, _) => added = s)
            .Returns(Task.CompletedTask);

        var response = await service.StartAsync(new StartHirePySessionRequestDto
        {
            Description = "Contact me at 0100 000 0000"
        }, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, response.Error!.statusCode);
        Assert.Contains("Contact details not allowed", response.Error!.message);
        Assert.Null(added);
    }

    [Fact]
    public async Task StartAsync_WhenDescriptionRedacted_StoresSafeText()
    {
        var moderation = new Mock<IContentModerationService>();
        moderation.Setup(m => m.ModerateAndEnforceAsync(
                It.IsAny<Guid>(), It.IsAny<ModerationSourceType>(), It.IsAny<Guid>(), It.IsAny<string?>(),
                It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentModerationResult
            {
                Action = ModerationAction.Redact,
                Status = ModerationStatus.Redacted,
                SafeText = "A bakery website [redacted]"
            });
        var (service, sessionRepo, _, _, _, _, _, _, _, _, _, _) = Build(_clientUserId, moderation: moderation);

        HirePySession? added = null;
        sessionRepo.Setup(r => r.AddAsync(It.IsAny<HirePySession>(), It.IsAny<CancellationToken>()))
            .Callback<HirePySession, CancellationToken>((s, _) => added = s)
            .Returns(Task.CompletedTask);

        var response = await service.StartAsync(new StartHirePySessionRequestDto
        {
            Description = "Call me on my private number 0100 000 0000"
        }, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("A bakery website [redacted]", added!.Description);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_RetryWhenProjectAlreadyCreated_LinksWithoutDuplicate()
    {
        var resumedSession = ResumeSession();
        var (service, _, _, projectService, generationService, _, _, backgroundJobClient, _, _, projectRepo, session) =
            Build(_clientUserId, resumedSession);

        var existingProject = new Project
        {
            Id = Guid.NewGuid(),
            ClientId = _clientUserId,
            Title = "Bakery Website",
            Description = "A polished marketing site for a bakery."
        };
        projectRepo.Setup(r => r.GetProjectsQuery()).Returns(new[] { existingProject }.AsQueryable());

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(existingProject.Id, session.ProjectId);
        Assert.Equal(HirePySessionStatus.ProjectCreated, session.Status);
        generationService.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        projectService.Verify(p => p.CreateForClientAsync(It.IsAny<CreateProjectRequestDto>(), _clientUserId, It.IsAny<CancellationToken>()), Times.Never);
        backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact]
    public async Task ProcessProjectGenerationAsync_RetryWhenProjectNotYetCreated_CreatesWithoutRegenerating()
    {
        var resumedSession = ResumeSession();
        var (service, _, _, projectService, generationService, _, _, backgroundJobClient, _, _, projectRepo, session) =
            Build(_clientUserId, resumedSession);

        projectRepo.Setup(r => r.GetProjectsQuery()).Returns(Array.Empty<Project>().AsQueryable());

        projectService.Setup(p => p.CreateForClientAsync(It.IsAny<CreateProjectRequestDto>(), _clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse.Success(_projectId));

        await service.ProcessProjectGenerationAsync(_sessionId, CancellationToken.None);

        Assert.Equal(_projectId, session.ProjectId);
        Assert.Equal(HirePySessionStatus.ProjectCreated, session.Status);
        generationService.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        projectService.Verify(p => p.CreateForClientAsync(
            It.Is<CreateProjectRequestDto>(d => d.Title == "Bakery Website"),
            _clientUserId,
            It.IsAny<CancellationToken>()), Times.Once);
        backgroundJobClient.Verify(c => c.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    private static HirePySession ResumeSession()
        => new()
        {
            Id = Guid.NewGuid(),
            ClientUserId = Guid.NewGuid(),
            Description = "I need a bakery website",
            Status = HirePySessionStatus.GeneratingProject,
            Title = "Bakery Website",
            GeneratedDescription = "A polished marketing site for a bakery.",
            CategoryId = ValidDraft.CategoryId,
            CategoryName = "Software Development",
            IsFixedPrice = true,
            BudgetMin = 500m,
            BudgetMax = 800m,
            Currency = "USD",
            EstimatedDurationDays = 30,
            Deadline = ValidDraft.Deadline,
            Complexity = "Medium",
            SkillIdsJson = JsonSerializer.Serialize(ValidDraft.SkillIds),
            SpecialtyIdsJson = JsonSerializer.Serialize(ValidDraft.SpecialtyIds),
            RequirementsJson = JsonSerializer.Serialize(ValidDraft.Requirements),
            FeaturesJson = JsonSerializer.Serialize(ValidDraft.Features),
            RisksJson = JsonSerializer.Serialize(ValidDraft.Risks),
            CreatedAt = DateTime.UtcNow
        };
}
