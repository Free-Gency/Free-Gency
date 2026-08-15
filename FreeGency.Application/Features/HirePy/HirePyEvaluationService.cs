using System.Text.Json;
using FreeGency.AI.HirePyInterview.Evaluation;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.HirePy.Dtos;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;

namespace FreeGency.Application.Features.HirePy;

public sealed class HirePyEvaluationService : IHirePyEvaluationService
{
    private const int DiscussionSummaryLimit = 20;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IHirePyEvaluatorAgent _agent;
    private readonly IProposalRankingService _rankingService;
    private readonly IHirePyEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    private readonly IHirePySessionRepository _sessionRepository;
    private readonly IHirePyInterviewRepository _interviewRepository;
    private readonly IHirePyEvaluationRepository _evaluationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMilestonePlanVersionRepository _planVersionRepository;

    public HirePyEvaluationService(
        IUnitOfWork unitOfWork,
        IHirePyEvaluatorAgent agent,
        IProposalRankingService rankingService,
        IHirePyEventPublisher eventPublisher,
        INotificationService notificationService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _agent = agent;
        _rankingService = rankingService;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _currentUser = currentUser;

        _sessionRepository = unitOfWork.Repository<IHirePySessionRepository, HirePySession>();
        _interviewRepository = unitOfWork.Repository<IHirePyInterviewRepository, HirePyInterview>();
        _evaluationRepository = unitOfWork.Repository<IHirePyEvaluationRepository, HirePyEvaluation>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _messageRepository = unitOfWork.Repository<IMessageRepository, Message>();
        _planVersionRepository = unitOfWork.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>();
    }

    public async Task ProcessPendingEvaluationsAsync(CancellationToken ct = default)
    {
        var sessions = await _sessionRepository.GetPendingEvaluationAsync(ct);
        foreach (var session in sessions)
        {
            try
            {
                await ProcessSessionEvaluationAsync(session, ct);
            }
            catch
            {
                // Never let one session break the whole poll cycle.
            }
        }
    }

    public async Task<ApiResponse<IReadOnlyList<HirePyEvaluationDto>>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null)
            return ApiResponse.Failure<IReadOnlyList<HirePyEvaluationDto>>(AppError.NotFound(nameof(HirePySession), sessionId));

        if (session.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure<IReadOnlyList<HirePyEvaluationDto>>(
                AppError.Forbidden("You do not own this HirePy session."));

        var evaluations = await _evaluationRepository.GetBySessionIdAsync(sessionId, ct);
        return ApiResponse.Success<IReadOnlyList<HirePyEvaluationDto>>(evaluations
            .OrderByDescending(e => e.OverallScore)
            .Select(MapDto)
            .ToList());
    }

    private async Task ProcessSessionEvaluationAsync(HirePySession session, CancellationToken ct)
    {
        if (session.ProjectId is null)
            return;

        if (session.Status is not (
                HirePySessionStatus.CandidateDiscussion or
                HirePySessionStatus.MilestonePlanning or
                HirePySessionStatus.CandidateEvaluation or
                HirePySessionStatus.CandidateComparison or
                HirePySessionStatus.RecommendationReady))
            return;

        var interviews = await _interviewRepository.GetBySessionIdAsync(session.Id, ct);
        if (interviews.Count == 0)
            return;

        // Wait until every interview has concluded (finalized milestone plan or failed).
        if (interviews.Any(IsActiveStatus))
            return;

        var finalized = interviews
            .Where(i => i.Status == HirePyInterviewStatus.MilestonePlanFinalized)
            .ToList();

        if (finalized.Count == 0)
        {
            await FailAsync(session, "No candidate finalized a milestone plan.", ct);
            return;
        }

        var project = await _projectRepository.GetByIdAsync(session.ProjectId.Value, ct);
        if (project is null)
        {
            await FailAsync(session, "The generated project was not found.", ct);
            return;
        }

        // Evaluations already persisted mean a previous run completed the AI evaluation
        // phase; resume from the last committed step instead of re-running the AI.
        var evaluations = (await _evaluationRepository.GetBySessionIdAsync(session.Id, ct)).ToList();
        if (evaluations.Count == 0)
        {
            if (session.Status != HirePySessionStatus.CandidateEvaluation)
            {
                session.Status = HirePySessionStatus.CandidateEvaluation;
                session.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);
                await PublishAsync(session, HirePyEventNames.CandidateEvaluationStarted, ct);
            }

            var proposals = (await _proposalRepository.GetByProjectIdAsync(project.Id, null, ct)).ToList();
            var ranking = await ResolveRankingScoresAsync(session, project.Id, proposals, ct);

            string? lastError = null;
            foreach (var interview in finalized)
            {
                var proposal = proposals.FirstOrDefault(p => p.Id == interview.ProjectProposalId);
                if (proposal is null)
                    continue;

                try
                {
                    evaluations.Add(await EvaluateCandidateAsync(
                        session, project, proposal, interview, ranking, ct));
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                }
            }

            if (evaluations.Count == 0)
            {
                await FailAsync(session,
                    string.IsNullOrWhiteSpace(lastError)
                        ? "Candidate evaluation failed."
                        : $"Candidate evaluation failed: {lastError}",
                    ct);
                return;
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAsync(session, HirePyEventNames.CandidateEvaluationCompleted, ct,
                candidateCount: evaluations.Count);
        }

        if (session.Status is not (
                HirePySessionStatus.CandidateComparison or
                HirePySessionStatus.RecommendationReady or
                HirePySessionStatus.WaitingForClientApproval))
        {
            session.Status = HirePySessionStatus.CandidateComparison;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAsync(session, HirePyEventNames.CandidateComparisonStarted, ct, candidateCount: evaluations.Count);
        }

        var winner = SelectWinner(evaluations);

        if (session.Status is not (
                HirePySessionStatus.RecommendationReady or
                HirePySessionStatus.WaitingForClientApproval))
        {
            ApplySelection(session, winner);
            session.Status = HirePySessionStatus.RecommendationReady;
            session.RecommendationCompletedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAsync(session, HirePyEventNames.RecommendationReady, ct);
        }

        if (session.Status != HirePySessionStatus.WaitingForClientApproval)
        {
            session.Status = HirePySessionStatus.WaitingForClientApproval;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAsync(session, HirePyEventNames.WaitingForClientApproval, ct);
        }

        await NotifyRecommendationReadyAsync(session, project.Id, winner.CandidateName, ct);
    }

    private async Task<HirePyEvaluation> EvaluateCandidateAsync(
        HirePySession session,
        Project project,
        ProjectProposal proposal,
        HirePyInterview interview,
        IReadOnlyDictionary<Guid, (int Score, int Position)> ranking,
        CancellationToken ct)
    {
        var candidateName = await GetCandidateNameAsync(interview.FreelancerUserId, ct);
        var plan = await _planVersionRepository.GetLatestByProposalIdAsync(proposal.Id, ct);
        var discussion = await BuildDiscussionSummaryAsync(interview, ct);

        var (rankingScore, rankingPosition) = ranking.TryGetValue(proposal.Id, out var r)
            ? r
            : (0, 0);

        var context = new HirePyEvaluationContext
        {
            CandidateName = candidateName,
            ProjectBrief = BuildProjectBrief(project),
            ProposalSummary = BuildProposalSummary(proposal),
            MilestonePlanSummary = BuildMilestonePlanSummary(project, plan),
            DiscussionSummary = discussion,
            RankingPosition = rankingPosition,
            RankingScore = rankingScore
        };

        var reply = await _agent.EvaluateAsync(context, ct);
        if (!reply.IsValid)
            throw new InvalidOperationException($"AI evaluation output was invalid for {candidateName}.");

        var evaluation = new HirePyEvaluation
        {
            Id = Guid.NewGuid(),
            HirePySessionId = session.Id,
            ProjectId = project.Id,
            ProjectProposalId = proposal.Id,
            FreelancerUserId = interview.FreelancerUserId,
            CandidateName = candidateName,
            RankingPosition = rankingPosition,
            RankingScore = rankingScore,
            TechnicalScore = reply.TechnicalScore,
            RequirementsScore = reply.RequirementsScore,
            ArchitectureScore = reply.ArchitectureScore,
            ImplementationScore = reply.ImplementationScore,
            MilestoneScore = reply.MilestoneScore,
            TimelineScore = reply.TimelineScore,
            BudgetScore = reply.BudgetScore,
            CommunicationScore = reply.CommunicationScore,
            RiskScore = reply.RiskScore,
            OverallScore = reply.OverallScore,
            StrengthsJson = JsonSerializer.Serialize(reply.Strengths),
            ConcernsJson = JsonSerializer.Serialize(reply.Concerns),
            RisksJson = JsonSerializer.Serialize(reply.Risks),
            Reason = reply.Reason,
            MilestoneSummary = BuildMilestonePlanSummary(project, plan),
            ProposedBudget = proposal.ProposedBudget,
            ProposedTimeline = proposal.ProposedTimeline,
            EvaluatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _evaluationRepository.AddAsync(evaluation, ct);
        return evaluation;
    }

    private async Task<string> BuildDiscussionSummaryAsync(HirePyInterview interview, CancellationToken ct)
    {
        var messages = await _messageRepository.GetLatestAsync(interview.ChatRoomId, DiscussionSummaryLimit, ct);
        var entries = messages
            .Where(m => !string.IsNullOrWhiteSpace(SafeText(m)))
            .Select(m => m.SenderDeveloperProfileId == interview.FreelancerDeveloperProfileId
                ? $"Candidate: {SafeText(m)}"
                : $"HirePy AI: {SafeText(m)}")
            .ToList();

        return entries.Count == 0
            ? "(no discussion messages)"
            : string.Join("\n", entries);
    }

    private static string? SafeText(Message message)
        => message.ModerationStatus == ModerationStatus.Redacted
           && !string.IsNullOrWhiteSpace(message.ModeratedText)
            ? message.ModeratedText
            : message.Text;

    private async Task<IReadOnlyDictionary<Guid, (int Score, int Position)>> ResolveRankingScoresAsync(
        HirePySession session,
        Guid projectId,
        List<ProjectProposal> proposals,
        CancellationToken ct)
    {
        var selected = ReadList<HirePySelectedCandidateDto>(session.SelectedCandidatesJson)
            .GroupBy(c => c.ProposalId)
            .ToDictionary(g => g.Key, g => g.First());

        Dictionary<Guid, double>? rankedScores = null;
        var rankResponse = await _rankingService.RankAsync(projectId, Math.Max(1, proposals.Count), ct);
        if (rankResponse.IsSuccess)
        {
            rankedScores = rankResponse.Data.RankedProposals
                .Where(r => Guid.TryParse(r.CandidateId, out _))
                .ToDictionary(r => Guid.Parse(r.CandidateId), r => r.OverallScore);
        }

        var result = new Dictionary<Guid, (int Score, int Position)>();
        foreach (var proposal in proposals)
        {
            var position = selected.TryGetValue(proposal.Id, out var candidate)
                ? Math.Max(1, candidate.Rank)
                : proposals.IndexOf(proposal) + 1;

            int score;
            if (rankedScores is not null && rankedScores.TryGetValue(proposal.Id, out var overall))
                score = (int)Math.Round(Math.Clamp(overall, 0, 100));
            else
                score = position == 1 ? 100 : Math.Max(0, 100 - (position - 1) * 20);

            result[proposal.Id] = (score, position);
        }

        return result;
    }

    private static HirePyEvaluation SelectWinner(List<HirePyEvaluation> evaluations)
        => evaluations
            .OrderByDescending(e => e.OverallScore)
            .ThenByDescending(e => e.MilestoneScore)
            .ThenByDescending(e => e.TechnicalScore)
            .ThenByDescending(e => e.RankingScore)
            .ThenBy(e => e.RankingPosition)
            .First();

    private static void ApplySelection(HirePySession session, HirePyEvaluation winner)
    {
        session.SelectedProposalId = winner.ProjectProposalId;
        session.SelectedFreelancerUserId = winner.FreelancerUserId;
        session.SelectedCandidateName = winner.CandidateName;
        session.DecisionReason = winner.Reason;
        session.MilestoneSummary = winner.MilestoneSummary;
        session.SelectedBudget = winner.ProposedBudget;
        session.SelectedTimeline = winner.ProposedTimeline;
    }

    private async Task NotifyRecommendationReadyAsync(HirePySession session, Guid projectId, string candidateName, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Your HirePy AI recommendation is ready",
            Body = $"HirePy evaluated the candidates and recommends {candidateName} for your project. Review and approve when ready.",
            Type = NotificationType.HirePyRecommendationReady,
            ProjectId = projectId,
            ActionUrl = $"/client/projects/{projectId}?tab=ai-interview",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task FailAsync(HirePySession session, string reason, CancellationToken ct)
    {
        session.Status = HirePySessionStatus.Failed;
        session.FailReason = reason;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await PublishAsync(session, HirePyEventNames.CandidateEvaluationFailed, ct,
            stage: HirePySessionStatus.Failed.ToString(),
            message: reason);

        await NotifyEvaluationFailedAsync(session, ct);
    }

    private async Task NotifyEvaluationFailedAsync(HirePySession session, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "HirePy candidate evaluation failed",
            Body = session.FailReason ?? "HirePy couldn't finish evaluating the candidates for your project. Please try again.",
            Type = NotificationType.HirePyFailed,
            ProjectId = session.ProjectId,
            ActionUrl = session.ProjectId.HasValue ? $"/client/projects/{session.ProjectId}" : "/client/hire-talent",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private Task PublishAsync(
        HirePySession session,
        string eventName,
        CancellationToken ct,
        string? stage = null,
        int? candidateCount = null,
        string? message = null)
        => _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            eventName,
            new HirePyEventPayload(
                session.Id,
                CoarseStatus(session.Status),
                stage ?? session.Status.ToString(),
                session.ProjectId,
                candidateCount,
                message),
            ct);

    private async Task<string> GetCandidateNameAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return "the candidate";
        return $"{user.FristName} {user.LastName}".Trim();
    }

    private static bool IsActiveStatus(HirePyInterview interview)
        => interview.Status is HirePyInterviewStatus.Started
            or HirePyInterviewStatus.MilestonePlanRequested
            or HirePyInterviewStatus.MilestonePlanReceived
            or HirePyInterviewStatus.MilestoneRevisionRequested;

    private static string BuildProjectBrief(Project project)
        => $"Title: {project.Title}\n" +
           $"Description: {project.Description}\n" +
           $"Budget: {project.Currency} {project.BudgetMin} - {project.BudgetMax}\n" +
           $"Deadline: {project.Deadline:yyyy-MM-dd}\n" +
           $"Estimated duration: {project.EstimatedDurationDays} days";

    private static string BuildProposalSummary(ProjectProposal proposal)
        => $"Cover letter: {proposal.CoverLetter}\n" +
           $"Approach: {proposal.Approach}\n" +
           $"Proposed timeline: {proposal.ProposedTimeline}\n" +
           $"Proposed budget: {proposal.ProposedBudget}";

    private static string BuildMilestonePlanSummary(Project project, MilestonePlanVersion? plan)
    {
        if (plan is null || plan.Items.Count == 0)
            return "(no finalized milestone plan)";

        var lines = new List<string>();
        var order = 0;
        foreach (var item in plan.Items)
        {
            order++;
            lines.Add($"{order}. {item.Title} — {project.Currency} {item.Amount:N0}" +
                      (item.DueDate.HasValue ? $" (due {item.DueDate:yyyy-MM-dd})" : string.Empty));
        }

        return string.Join("\n", lines);
    }

    private static HirePyEvaluationDto MapDto(HirePyEvaluation e)
        => new()
        {
            ProposalId = e.ProjectProposalId,
            FreelancerUserId = e.FreelancerUserId,
            CandidateName = e.CandidateName,
            RankingPosition = e.RankingPosition,
            RankingScore = e.RankingScore,
            TechnicalScore = e.TechnicalScore,
            RequirementsScore = e.RequirementsScore,
            ArchitectureScore = e.ArchitectureScore,
            ImplementationScore = e.ImplementationScore,
            MilestoneScore = e.MilestoneScore,
            TimelineScore = e.TimelineScore,
            BudgetScore = e.BudgetScore,
            CommunicationScore = e.CommunicationScore,
            RiskScore = e.RiskScore,
            OverallScore = e.OverallScore,
            Strengths = ReadList<string>(e.StrengthsJson),
            Concerns = ReadList<string>(e.ConcernsJson),
            Risks = ReadList<string>(e.RisksJson),
            Reason = e.Reason,
            MilestoneSummary = e.MilestoneSummary,
            ProposedBudget = e.ProposedBudget,
            ProposedTimeline = e.ProposedTimeline,
            EvaluatedAt = e.EvaluatedAt
        };

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
