using System.Text.Json;
using FreeGency.AI.ProjectDrafting;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.HirePy.Dtos;
using FreeGency.Application.Features.Moderation.DTOs;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.Projects.DTOs;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FluentValidation;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Application.Features.HirePy;

public class HirePySessionService : IHirePySessionService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectService _projectService;
    private readonly IProjectGenerationService _projectGenerationService;
    private readonly INotificationService _notificationService;
    private readonly IHirePyEventPublisher _eventPublisher;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IValidator<CreateProjectRequestDto> _createProjectValidator;
    private readonly IContentModerationService _contentModerationService;
    private readonly IHirePySessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;

    public HirePySessionService(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IProjectService projectService,
        IProjectGenerationService projectGenerationService,
        INotificationService notificationService,
        IHirePyEventPublisher eventPublisher,
        IBackgroundJobClient backgroundJobClient,
        IValidator<CreateProjectRequestDto> createProjectValidator,
        IContentModerationService contentModerationService)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _projectService = projectService;
        _projectGenerationService = projectGenerationService;
        _notificationService = notificationService;
        _eventPublisher = eventPublisher;
        _backgroundJobClient = backgroundJobClient;
        _createProjectValidator = createProjectValidator;
        _contentModerationService = contentModerationService;
        _sessionRepository = unitOfWork.Repository<IHirePySessionRepository, HirePySession>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
    }

    public async Task<ApiResponse<StartHirePySessionResponseDto>> StartAsync(
        StartHirePySessionRequestDto request,
        CancellationToken ct = default)
    {
        if (_currentUser.UserId == Guid.Empty)
            return ApiResponse.Failure<StartHirePySessionResponseDto>(AppError.Unauthorized());

        if (string.IsNullOrWhiteSpace(request.Description))
            return ApiResponse.Failure<StartHirePySessionResponseDto>(
                AppError.Validation("A project description is required."));

        var description = request.Description.Trim();

        // Moderation is part of the write path (same pattern as chat/feedback): block or
        // redact descriptions that violate community rules before they ever reach the AI.
        var sessionId = Guid.NewGuid();
        var moderation = await ModerateDescriptionAsync(_currentUser.UserId, sessionId, description, ct);
        if (moderation.Status == ModerationStatus.Hidden)
            return ApiResponse.Failure<StartHirePySessionResponseDto>(
                AppError.Validation(moderation.WarningMessage
                    ?? "Your project description violates FreeGency community rules."));

        var session = new HirePySession
        {
            Id = sessionId,
            ClientUserId = _currentUser.UserId,
            Description = moderation.Status == ModerationStatus.Redacted
                          && !string.IsNullOrWhiteSpace(moderation.SafeText)
                ? moderation.SafeText
                : description,
            Status = HirePySessionStatus.GeneratingProject,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId.ToString()
        };

        await _sessionRepository.AddAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            HirePyEventNames.Started,
            new HirePyEventPayload(
                session.Id,
                "Processing",
                HirePySessionStatus.GeneratingProject.ToString()),
            ct);

        _backgroundJobClient.Enqueue<IHirePySessionService>(
            s => s.ProcessProjectGenerationAsync(session.Id, CancellationToken.None));

        return ApiResponse.Success(new StartHirePySessionResponseDto
        {
            SessionId = session.Id,
            Status = "Processing",
            Stage = HirePySessionStatus.GeneratingProject.ToString()
        }, "HirePy session started. Project generation is in progress.");
    }

    public async Task ProcessProjectGenerationAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null || session.Status != HirePySessionStatus.GeneratingProject)
            return;

        // A persisted draft means a previous run already committed the AI output. Resume from
        // the last committed step instead of regenerating, so retries never re-run the AI and
        // never create a duplicate project.
        var isResume = session.Title is not null;
        GeneratedProjectDraft? draft = null;

        if (!isResume)
        {
            await _eventPublisher.PublishClientEventAsync(
                session.ClientUserId,
                HirePyEventNames.ProjectGenerationStarted,
                new HirePyEventPayload(session.Id, "Processing", HirePySessionStatus.GeneratingProject.ToString()),
                ct);

            try
            {
                draft = await _projectGenerationService.GenerateAsync(session.Description, ct);
            }
            catch (Exception ex)
            {
                await FailAsync(session, $"AI project generation failed: {ex.Message}", ct);
                return;
            }

            await _eventPublisher.PublishClientEventAsync(
                session.ClientUserId,
                HirePyEventNames.ProjectGenerated,
                new HirePyEventPayload(session.Id, "Processing", HirePySessionStatus.GeneratingProject.ToString()),
                ct);

            if (draft.NeedsManualCategoryReview || draft.CategoryId is null || draft.SkillIds.Count == 0)
            {
                await FailAsync(session,
                    "AI could not map the request to a supported category and skills. Please describe the project differently.",
                    ct);
                return;
            }

            ApplyDraft(session, draft);

            // Commit the draft before the non-idempotent project creation. If the process
            // crashes between project creation and the session update below, the retry can
            // detect the already-created project and link it instead of duplicating it.
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            draft = RebuildDraft(session);
        }

        var createRequest = MapToCreateRequest(draft);

        var validation = await _createProjectValidator.ValidateAsync(createRequest, ct);
        if (!validation.IsValid)
        {
            await FailAsync(session,
                "AI output failed normal project validation: "
                + string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ct);
            return;
        }

        if (session.ProjectId is null)
        {
            if (isResume)
            {
                // Retry recovery: a previous attempt may have created the project but crashed
                // before persisting the link. Adopt that project instead of creating a duplicate.
                var existingProject = await FindMatchingProjectAsync(session, ct);
                if (existingProject is not null)
                {
                    session.ProjectId = existingProject.Id;
                }
                else
                {
                    var result = await _projectService.CreateForClientAsync(createRequest, session.ClientUserId, ct);
                    if (!result.IsSuccess)
                    {
                        await FailAsync(session, $"Project creation failed: {result.Error?.message}", ct);
                        return;
                    }

                    session.ProjectId = result.Data;
                }
            }
            else
            {
                var result = await _projectService.CreateForClientAsync(createRequest, session.ClientUserId, ct);
                if (!result.IsSuccess)
                {
                    await FailAsync(session, $"Project creation failed: {result.Error?.message}", ct);
                    return;
                }

                session.ProjectId = result.Data;
            }
        }

        session.Status = HirePySessionStatus.ProjectCreated;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            HirePyEventNames.ProjectCreated,
            new HirePyEventPayload(session.Id, "Processing", HirePySessionStatus.ProjectCreated.ToString(), session.ProjectId),
            ct);

        await NotifyProjectCreatedAsync(session, ct);

        _backgroundJobClient.Enqueue<IHirePyCandidateService>(
            c => c.ProcessRankingAndInvitationsAsync(session.Id, CancellationToken.None));
    }

    public async Task<ApiResponse<HirePySessionDto>> GetAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null)
            return ApiResponse.Failure<HirePySessionDto>(AppError.NotFound(nameof(HirePySession), sessionId));

        if (session.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure<HirePySessionDto>(AppError.Forbidden("You do not own this HirePy session."));

        return ApiResponse.Success(MapDto(session));
    }

    public async Task<ApiResponse<IReadOnlyList<HirePySessionDto>>> GetMineAsync(CancellationToken ct = default)
    {
        var sessions = await _sessionRepository.GetByClientUserIdAsync(_currentUser.UserId, ct);
        return ApiResponse.Success<IReadOnlyList<HirePySessionDto>>(sessions.Select(MapDto).ToList());
    }

    private async Task<Project?> FindMatchingProjectAsync(HirePySession session, CancellationToken ct)
    {
        if (session.ProjectId is not null)
            return await _projectRepository.GetByIdAsync(session.ProjectId.Value, ct);

        return await _projectRepository.GetProjectsQuery()
            .Where(p => !p.IsDeleted
                        && p.ClientId == session.ClientUserId
                        && p.Title == session.Title
                        && p.Description == session.GeneratedDescription)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    private static GeneratedProjectDraft RebuildDraft(HirePySession session)
        => new()
        {
            Title = session.Title ?? string.Empty,
            Description = session.GeneratedDescription ?? string.Empty,
            CategoryId = session.CategoryId,
            CategoryName = session.CategoryName,
            IsFixedPrice = session.IsFixedPrice,
            BudgetMin = session.BudgetMin ?? 0m,
            BudgetMax = session.BudgetMax ?? 0m,
            Currency = session.Currency ?? string.Empty,
            EstimatedDurationDays = session.EstimatedDurationDays,
            Deadline = session.Deadline,
            Complexity = session.Complexity,
            SkillIds = ReadList<Guid>(session.SkillIdsJson),
            SpecialtyIds = ReadList<Guid>(session.SpecialtyIdsJson),
            Requirements = ReadList<string>(session.RequirementsJson),
            Features = ReadList<string>(session.FeaturesJson),
            Risks = ReadList<string>(session.RisksJson)
        };

    private async Task<ContentModerationResult> ModerateDescriptionAsync(
        Guid clientUserId,
        Guid sessionId,
        string description,
        CancellationToken ct)
    {
        try
        {
            var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(clientUserId, ct);
            return await _contentModerationService.ModerateAndEnforceAsync(
                clientUserId,
                ModerationSourceType.HirePyProjectDescription,
                sessionId,
                description,
                "hirepy-project-description",
                clientProfileId,
                null,
                ct);
        }
        catch
        {
            // Fail-open: a moderation outage must never block a legitimate project start.
            return new ContentModerationResult
            {
                Action = ModerationAction.Allow,
                Status = ModerationStatus.Visible,
                SafeText = description
            };
        }
    }

    private async Task FailAsync(HirePySession session, string reason, CancellationToken ct)
    {
        session.Status = HirePySessionStatus.Failed;
        session.FailReason = reason;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task NotifyProjectCreatedAsync(HirePySession session, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Your HirePy project is ready",
            Body = $"Your AI-generated project \"{session.Title}\" has been created.",
            Type = NotificationType.HirePyProjectCreated,
            ProjectId = session.ProjectId,
            ActionUrl = $"/client/projects/{session.ProjectId}",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private static void ApplyDraft(HirePySession session, GeneratedProjectDraft draft)
    {
        session.Title = draft.Title;
        session.GeneratedDescription = draft.Description;
        session.CategoryId = draft.CategoryId;
        session.CategoryName = draft.CategoryName;
        session.IsFixedPrice = draft.IsFixedPrice;
        session.BudgetMin = draft.BudgetMin;
        session.BudgetMax = draft.BudgetMax;
        session.Currency = draft.Currency;
        session.EstimatedDurationDays = draft.EstimatedDurationDays;
        session.Deadline = draft.Deadline;
        session.Complexity = draft.Complexity;
        session.SkillIdsJson = JsonSerializer.Serialize(draft.SkillIds);
        session.SpecialtyIdsJson = JsonSerializer.Serialize(draft.SpecialtyIds);
        session.RequirementsJson = JsonSerializer.Serialize(draft.Requirements);
        session.FeaturesJson = JsonSerializer.Serialize(draft.Features);
        session.RisksJson = JsonSerializer.Serialize(draft.Risks);
    }

    private static CreateProjectRequestDto MapToCreateRequest(GeneratedProjectDraft draft)
        => new()
        {
            Title = draft.Title,
            Description = draft.Description,
            CategoryId = draft.CategoryId!.Value,
            IsFixedPrice = draft.IsFixedPrice,
            BudgetMin = draft.BudgetMin,
            BudgetMax = draft.BudgetMax,
            Currency = draft.Currency,
            EstimatedDurationDays = draft.EstimatedDurationDays,
            SkillIds = draft.SkillIds,
            SpecialtyIds = draft.SpecialtyIds,
        };

    private static HirePySessionDto MapDto(HirePySession s)
        => new()
        {
            Id = s.Id,
            ClientUserId = s.ClientUserId,
            ProjectId = s.ProjectId,
            Description = s.Description,
            Status = CoarseStatus(s.Status),
            Stage = s.Status.ToString(),
            FailReason = s.FailReason,
            Title = s.Title,
            GeneratedDescription = s.GeneratedDescription,
            CategoryId = s.CategoryId,
            CategoryName = s.CategoryName,
            IsFixedPrice = s.IsFixedPrice,
            BudgetMin = s.BudgetMin,
            BudgetMax = s.BudgetMax,
            Currency = s.Currency,
            Deadline = s.Deadline,
            EstimatedDurationDays = s.EstimatedDurationDays,
            Complexity = s.Complexity,
            SkillIds = ReadList<Guid>(s.SkillIdsJson),
            SpecialtyIds = ReadList<Guid>(s.SpecialtyIdsJson),
            Requirements = ReadList<string>(s.RequirementsJson),
            Features = ReadList<string>(s.FeaturesJson),
            Risks = ReadList<string>(s.RisksJson),
            SelectedCandidates = ReadList<HirePySelectedCandidateDto>(s.SelectedCandidatesJson),
            SelectedProposalId = s.SelectedProposalId,
            SelectedFreelancerUserId = s.SelectedFreelancerUserId,
            SelectedCandidateName = s.SelectedCandidateName,
            DecisionReason = s.DecisionReason,
            MilestoneSummary = s.MilestoneSummary,
            SelectedBudget = s.SelectedBudget,
            SelectedTimeline = s.SelectedTimeline,
            RecommendationCompletedAt = s.RecommendationCompletedAt,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            CompletedAt = s.CompletedAt,
            AcceptedPlanVersionId = s.AcceptedPlanVersionId,
        };

    private static string CoarseStatus(HirePySessionStatus status)
        => status switch
        {
            HirePySessionStatus.Completed => "Completed",
            HirePySessionStatus.Failed => "Failed",
            HirePySessionStatus.Cancelled => "Cancelled",
            _ => "Processing"
        };

    private static List<T> ReadList<T>(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<T>>(json, JsonOpts) ?? [];
}
