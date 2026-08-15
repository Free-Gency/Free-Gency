using System.Text.Json;
using FreeGency.Application.Features.HirePy.Dtos;

namespace FreeGency.Application.Features.HirePy;

/// <summary>
/// Presents the final client-safe recommendation and executes the client's "Approve &amp; Hire"
/// decision. All final-hire logic is delegated to the existing hiring service; this service only
/// verifies ownership, eligibility and availability before the hire. The AI agent never executes
/// the final hire without the client's approval.
/// </summary>
public sealed class HirePyApprovalService : IHirePyApprovalService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMilestoneService _milestoneService;
    private readonly IHirePyEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;

    private readonly IHirePySessionRepository _sessionRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IHirePyEvaluationRepository _evaluationRepository;
    private readonly IMilestonePlanVersionRepository _planVersionRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUserRepository _userRepository;

    public HirePyApprovalService(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IMilestoneService milestoneService,
        IHirePyEventPublisher eventPublisher,
        INotificationService notificationService)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _milestoneService = milestoneService;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;

        _sessionRepository = unitOfWork.Repository<IHirePySessionRepository, HirePySession>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _evaluationRepository = unitOfWork.Repository<IHirePyEvaluationRepository, HirePyEvaluation>();
        _planVersionRepository = unitOfWork.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>();
        _skillRepository = unitOfWork.Repository<ISkillRepository, Skill>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
    }

    public async Task<ApiResponse<HirePyRecommendationDto>> GetRecommendationAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null)
            return ApiResponse.Failure<HirePyRecommendationDto>(AppError.NotFound(nameof(HirePySession), sessionId));

        if (session.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure<HirePyRecommendationDto>(AppError.Forbidden("You do not own this HirePy session."));

        if (session.ProjectId is null || session.SelectedProposalId is null || session.SelectedFreelancerUserId is null)
            return ApiResponse.Failure<HirePyRecommendationDto>(
                AppError.Validation("No recommendation is available for this session yet."));

        var project = await _projectRepository.GetByIdAsync(session.ProjectId.Value, ct);
        if (project is null)
            return ApiResponse.Failure<HirePyRecommendationDto>(AppError.NotFound(nameof(Project), session.ProjectId.Value));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<HirePyRecommendationDto>(AppError.Forbidden("You do not own this project."));

        var proposal = await _proposalRepository.GetByIdAsync(session.SelectedProposalId.Value, ct);
        var evaluations = await _evaluationRepository.GetByProposalIdAsync(session.SelectedProposalId.Value, ct);
        var evaluation = evaluations.FirstOrDefault(e => e.FreelancerUserId == session.SelectedFreelancerUserId)
            ?? evaluations.FirstOrDefault();

        var plan = await _planVersionRepository.GetLatestByProposalIdAsync(session.SelectedProposalId.Value, ct);
        var skillNames = await ResolveSkillNamesAsync(session, ct);

        return ApiResponse.Success(BuildDto(session, project, proposal, evaluation, skillNames));
    }

    public async Task<ApiResponse> ApproveAndHireAsync(Guid sessionId, CancellationToken ct = default)
    {
        // 1. Authenticate the client.
        if (_currentUser.UserId == Guid.Empty)
            return ApiResponse.Failure(AppError.Unauthorized());

        // 2/3. Verify the HirePy session and its ownership.
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(HirePySession), sessionId));

        if (session.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("You do not own this HirePy session."));

        if (session.Status == HirePySessionStatus.Completed)
            return ApiResponse.Failure(AppError.Conflict("This recommendation has already been approved and the candidate hired."));

        // A crash may have interrupted the finalization after the plan was accepted. The plan
        // accept step is not idempotent (it guards on a pending plan status), so a resume skips
        // the eligibility re-checks and lets the atomic completion transition decide the outcome.
        if (session.Status == HirePySessionStatus.Hiring)
        {
            if (session.ProjectId is null || session.SelectedProposalId is null || session.SelectedFreelancerUserId is null)
                return ApiResponse.Failure(AppError.Validation("No recommendation is available to resume."));

            var inFlightProject = await _projectRepository.GetByIdAsync(session.ProjectId.Value, ct);
            if (inFlightProject is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), session.ProjectId.Value));

            var inFlightPlan = await _planVersionRepository.GetLatestByProposalIdAsync(session.SelectedProposalId.Value, ct);
            if (inFlightPlan is null || inFlightPlan.IsDeleted || inFlightPlan.ProposalId != session.SelectedProposalId.Value)
                return ApiResponse.Failure(AppError.Conflict("The hire could not be resumed because the milestone plan no longer exists."));

            return await FinishHiringAsync(session, inFlightProject, inFlightPlan, ct);
        }

        // 4. Verify the recommendation exists.
        if (session.ProjectId is null || session.SelectedProposalId is null || session.SelectedFreelancerUserId is null)
            return ApiResponse.Failure(AppError.Validation("No recommendation is available to approve yet."));

        if (session.RecommendationCompletedAt is null)
            return ApiResponse.Failure(AppError.Validation("The recommendation is not complete yet."));

        // 5. Verify the project and its ownership.
        var project = await _projectRepository.GetByIdAsync(session.ProjectId.Value, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), session.ProjectId.Value));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("You do not own this project."));

        // 7. Verify the project is still available.
        if (project.Status != ProjectStatus.Open || project.AssignedUserId is not null || project.AssignedTeamId is not null)
            return ApiResponse.Failure(AppError.Conflict("This project is no longer available for hire."));

        // 6. Verify the selected candidate is still eligible.
        var proposal = await _proposalRepository.GetByIdAsync(session.SelectedProposalId.Value, ct);
        if (proposal is null || proposal.IsDeleted)
            return ApiResponse.Failure(AppError.Conflict("The recommended candidate is no longer available."));

        if (proposal.ProjectId != project.Id)
            return ApiResponse.Failure(AppError.Conflict("The recommended proposal no longer belongs to this project."));

        if (proposal.Status != ProposalStatus.InDiscussion)
            return ApiResponse.Failure(AppError.Conflict("The recommended candidate is no longer eligible."));

        if (proposal.UserId != session.SelectedFreelancerUserId)
            return ApiResponse.Failure(AppError.Conflict("The recommended candidate no longer matches this proposal."));

        // The recommended freelancer account must still exist.
        var freelancer = await _userRepository.GetByIdAsync(session.SelectedFreelancerUserId.Value, ct);
        if (freelancer is null || freelancer.IsDeleted)
            return ApiResponse.Failure(AppError.Conflict("The recommended freelancer is no longer available."));

        var plan = await _planVersionRepository.GetLatestByProposalIdAsync(session.SelectedProposalId.Value, ct);
        if (plan is null || plan.IsDeleted || plan.ProposalId != session.SelectedProposalId.Value)
            return ApiResponse.Failure(AppError.Conflict("The recommended candidate has no agreed milestone plan."));

        if (plan.Status != PlanVersionStatus.Proposed)
            return ApiResponse.Failure(AppError.Conflict("The recommended candidate's milestone plan is no longer pending."));

        // 8. Atomically claim the Hiring status. The conditional update is the concurrency gate:
        // only one request can transition WaitingForClientApproval → Hiring, so concurrent or
        // repeated approvals can never double-hire the candidate.
        var claimed = await _sessionRepository.TryTransitionStatusAsync(
            session.Id,
            HirePySessionStatus.WaitingForClientApproval,
            HirePySessionStatus.Hiring,
            ct);
        if (!claimed)
            return ApiResponse.Failure(AppError.Conflict(
                "The hire has already been processed for this recommendation."));

        session.Status = HirePySessionStatus.Hiring; // keep the tracked entity consistent
        await PublishAsync(session, HirePyEventNames.HiringStarted, ct);

        try
        {
            return await FinishHiringAsync(session, project, plan, ct);
        }
        catch (Exception ex)
        {
            await RestoreAwaitingApprovalAsync(session, ct);
            return ApiResponse.Failure(AppError.Failure(ex));
        }
    }

    /// <summary>
    /// Runs the finalization from the Hiring state. The plan is accepted if it is still
    /// pending (first attempt or a crash before the accept committed), then the atomic
    /// Hiring → Completed transition is the completion gate: exactly one finalizer (the
    /// original request or a retried one) wins it and sends the completion signals.
    /// </summary>
    private async Task<ApiResponse> FinishHiringAsync(
        HirePySession session,
        Project project,
        MilestonePlanVersion plan,
        CancellationToken ct)
    {
        if (plan.Status != PlanVersionStatus.Accepted)
        {
            ApiResponse accept;
            try
            {
                accept = await _milestoneService.AcceptPlanAsync(plan.Id, ct);
            }
            catch (Exception ex)
            {
                await RestoreAwaitingApprovalAsync(session, ct);
                return ApiResponse.Failure(AppError.Failure(ex));
            }

            if (!accept.IsSuccess)
            {
                await RestoreAwaitingApprovalAsync(session, ct);
                return ApiResponse.Failure(accept.Error!, accept.Message);
            }
        }

        var completed = await _sessionRepository.TryTransitionStatusAsync(
            session.Id,
            HirePySessionStatus.Hiring,
            HirePySessionStatus.Completed,
            ct);
        if (!completed)
            return ApiResponse.Success("The hire for this recommendation has already been completed.");

        // Best-effort tracked save: capture the finalization details. The status itself was
        // already committed atomically by the conditional update above.
        session.Status = HirePySessionStatus.Completed;
        session.CompletedAt ??= DateTime.UtcNow;
        session.AcceptedPlanVersionId ??= plan.Id;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await PublishAsync(session, HirePyEventNames.HiringCompleted, ct,
            stage: HirePySessionStatus.Completed.ToString(),
            message: "Hired");
        await PublishAsync(session, HirePyEventNames.HirePyCompleted, ct,
            stage: HirePySessionStatus.Completed.ToString(),
            message: "Hired");

        await NotifyHiringCompletedAsync(session, project, ct);

        return ApiResponse.Success(
            $"Milestone plan accepted — {session.SelectedCandidateName ?? "the recommended candidate"} has been hired for \"{project.Title}\".");
    }

    private async Task NotifyHiringCompletedAsync(HirePySession session, Project project, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "HirePy hire completed",
            Body = $"You hired {session.SelectedCandidateName ?? "the recommended candidate"} for \"{project.Title}\". Fund Milestone #1 to start work.",
            Type = NotificationType.HirePyHiringCompleted,
            ProjectId = project.Id,
            ActionUrl = $"/client/projects/{project.Id}?tab=milestones",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task RestoreAwaitingApprovalAsync(HirePySession session, CancellationToken ct)
    {
        session.Status = HirePySessionStatus.WaitingForClientApproval;
        session.UpdatedAt = DateTime.UtcNow;
        // The Hiring status was written with an untracked conditional update, so the tracked
        // entity cannot roll it back — revert the status in the database directly.
        await _sessionRepository.TryTransitionStatusAsync(
            session.Id,
            HirePySessionStatus.Hiring,
            HirePySessionStatus.WaitingForClientApproval,
            ct);
    }

    private async Task<IReadOnlyList<string>> ResolveSkillNamesAsync(HirePySession session, CancellationToken ct)
    {
        var ids = ReadList<Guid>(session.SkillIdsJson);
        if (ids.Count == 0)
            return [];

        var skills = (await _skillRepository.GetByIdsAsync(ids, ct)).ToList();
        var byId = skills.ToDictionary(s => s.Id, s => s.Name);
        return ids.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
    }

    private static HirePyRecommendationDto BuildDto(
        HirePySession session,
        Project project,
        ProjectProposal? proposal,
        HirePyEvaluation? evaluation,
        IReadOnlyList<string> skillNames)
    {
        var proposedBudget = proposal?.ProposedBudget ?? session.SelectedBudget;
        var proposedTimeline = proposal?.ProposedTimeline ?? session.SelectedTimeline;

        return new HirePyRecommendationDto
        {
            SessionId = session.Id,
            ProjectId = project.Id,
            Stage = session.Status.ToString(),
            RecommendationCompletedAt = session.RecommendationCompletedAt,
            Project = new HirePyRecommendationProjectDto
            {
                Title = project.Title,
                Description = project.Description,
                Requirements = ReadList<string>(session.RequirementsJson),
                Skills = [.. skillNames],
                BudgetMin = project.BudgetMin,
                BudgetMax = project.BudgetMax,
                Currency = project.Currency,
                IsFixedPrice = project.IsFixedPrice,
                Deadline = project.Deadline,
                EstimatedDurationDays = project.EstimatedDurationDays,
                CategoryName = session.CategoryName
            },
            Freelancer = new HirePyRecommendationFreelancerDto
            {
                FreelancerUserId = session.SelectedFreelancerUserId!.Value,
                CandidateName = session.SelectedCandidateName ?? evaluation?.CandidateName ?? "the candidate",
                RankingPosition = evaluation?.RankingPosition ?? 0,
                RankingScore = evaluation?.RankingScore ?? 0
            },
            Proposal = new HirePyRecommendationProposalDto
            {
                ProposalId = session.SelectedProposalId!.Value,
                CoverLetter = proposal?.CoverLetter,
                Approach = proposal?.Approach,
                ProposedBudget = proposedBudget,
                ProposedTimeline = proposedTimeline
            },
            Evaluation = new HirePyRecommendationEvaluationDto
            {
                OverallScore = evaluation?.OverallScore ?? 0,
                TechnicalScore = evaluation?.TechnicalScore ?? 0,
                RequirementsScore = evaluation?.RequirementsScore ?? 0,
                ArchitectureScore = evaluation?.ArchitectureScore ?? 0,
                ImplementationScore = evaluation?.ImplementationScore ?? 0,
                MilestoneScore = evaluation?.MilestoneScore ?? 0,
                TimelineScore = evaluation?.TimelineScore ?? 0,
                BudgetScore = evaluation?.BudgetScore ?? 0,
                CommunicationScore = evaluation?.CommunicationScore ?? 0,
                RiskScore = evaluation?.RiskScore ?? 0,
                Strengths = ReadList<string>(evaluation?.StrengthsJson),
                Concerns = ReadList<string>(evaluation?.ConcernsJson),
                Risks = ReadList<string>(evaluation?.RisksJson),
                Reason = evaluation?.Reason ?? session.DecisionReason,
                MilestoneSummary = evaluation?.MilestoneSummary ?? session.MilestoneSummary,
                BudgetCompatibility = BuildBudgetCompatibility(proposedBudget, project.BudgetMin, project.BudgetMax, project.Currency)
            }
        };
    }

    private static string? BuildBudgetCompatibility(decimal? proposed, decimal min, decimal max, string currency)
    {
        if (proposed is null)
            return null;

        if (proposed >= min && proposed <= max)
            return $"Within the {currency} {min:N0} - {max:N0} budget range.";

        if (proposed < min)
            return $"Below the {currency} {min:N0} minimum budget.";

        return $"Exceeds the {currency} {max:N0} maximum budget by {currency} {proposed - max:N0}.";
    }

    private Task PublishAsync(
        HirePySession session,
        string eventName,
        CancellationToken ct,
        string? stage = null,
        string? message = null)
        => _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            eventName,
            new HirePyEventPayload(
                session.Id,
                CoarseStatus(session.Status),
                stage ?? session.Status.ToString(),
                session.ProjectId,
                null,
                message),
            ct);

    private static List<T> ReadList<T>(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<T>>(json, JsonOpts) ?? [];

    private static string CoarseStatus(HirePySessionStatus status)
        => status switch
        {
            HirePySessionStatus.Completed => "Completed",
            HirePySessionStatus.Failed => "Failed",
            HirePySessionStatus.Cancelled => "Cancelled",
            _ => "Processing"
        };
}
