using System.Text.Json;
using FreeGency.AI.Ranking.ProposalRanking;
using FreeGency.Application.Features.HirePy.Dtos;
using FreeGency.Application.Features.ProjectInvitations.Dtos;
using FreeGency.Domain.Interfaces.Repositories.Teams;

namespace FreeGency.Application.Features.HirePy;

public sealed class HirePyCandidateService : IHirePyCandidateService
{
    private const int TopCandidateCount = 5;

    private const string InvitationMessage =
        "Hi! Your proposal for this project stood out. We'd like to discuss the scope and milestones with you.";

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IProposalRankingService _proposalRankingService;
    private readonly IProjectInvitationService _invitationService;
    private readonly IProjectService _projectService;
    private readonly IHirePyEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly IHirePyInterviewService _interviewService;

    private readonly IHirePySessionRepository _sessionRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeveloperProfileRepository _developerProfileRepository;
    private readonly ITeamRepository _teamRepository;

    public HirePyCandidateService(
        IUnitOfWork unitOfWork,
        IProposalRankingService proposalRankingService,
        IProjectInvitationService invitationService,
        IProjectService projectService,
        IHirePyEventPublisher eventPublisher,
        INotificationService notificationService,
        IHirePyInterviewService interviewService)
    {
        _unitOfWork = unitOfWork;
        _proposalRankingService = proposalRankingService;
        _invitationService = invitationService;
        _projectService = projectService;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _interviewService = interviewService;
        _sessionRepository = unitOfWork.Repository<IHirePySessionRepository, HirePySession>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _invitationRepository = unitOfWork.Repository<IProjectInvitationRepository, ProjectInvitation>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        _teamRepository = unitOfWork.Repository<ITeamRepository, Team>();
    }

    public async Task ProcessRankingAndInvitationsAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null || session.ProjectId is null)
            return;

        // Idempotency guard: once invitations have been sent (or the session is terminal),
        // a retry must not re-run ranking or send duplicate invitations.
        if (session.Status is not (
                HirePySessionStatus.ProjectCreated or
                HirePySessionStatus.RankingCandidates or
                HirePySessionStatus.TopCandidatesSelected or
                HirePySessionStatus.InvitingCandidates))
            return;

        session.Status = HirePySessionStatus.RankingCandidates;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await PublishAsync(session, HirePyEventNames.RankingStarted, ct);

        var project = await _projectRepository.GetByIdAsync(session.ProjectId.Value, ct);
        if (project is null)
        {
            await FailAsync(session, "The generated project was not found.", ct);
            await NotifyFailedAsync(session, ct);
            return;
        }

        if (project.Status == ProjectStatus.Draft)
        {
            var published = await _projectService.PublishForClientAsync(project.Id, session.ClientUserId, ct);
            if (!published.IsSuccess)
            {
                await FailAsync(session, $"Publishing the project failed: {published.Error?.message}", ct);
                await NotifyFailedAsync(session, ct);
                return;
            }
        }
        else if (project.Status != ProjectStatus.Open)
        {
            await FailAsync(session, "The project is not open for candidates.", ct);
            await NotifyFailedAsync(session, ct);
            return;
        }

        var proposals = (await _proposalRepository.GetByProjectIdAsync(project.Id, null, ct)).ToList();
        var rankResponse = await _proposalRankingService.RankAsync(project.Id, Math.Max(1, proposals.Count), ct);
        if (!rankResponse.IsSuccess)
        {
            await FailAsync(session, $"Candidate ranking failed: {rankResponse.Error?.message}", ct);
            await NotifyFailedAsync(session, ct);
            return;
        }

        var ranked = rankResponse.Data.RankedProposals;
        await PublishAsync(session, HirePyEventNames.RankingCompleted, ct, candidateCount: ranked.Count);

        var existingInvitations = await _invitationRepository.GetByProjectIdAsync(project.Id, ct);
        var selected = await SelectTopCandidatesAsync(project, ranked, proposals, existingInvitations, ct);

        session.SelectedCandidatesJson = JsonSerializer.Serialize(selected);
        await _unitOfWork.SaveChangesAsync(ct);

        if (selected.Count == 0)
        {
            // A retry after invitations were already sent is not a failure.
            if (existingInvitations.Any(i => i.Status == ProjectInvitationStatus.Pending))
            {
                session.Status = HirePySessionStatus.WaitingForCandidates;
                session.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);
                return;
            }

            await FailAsync(session, "No eligible candidates were found for your project.", ct);
            await NotifyNoCandidatesAsync(session, ct);
            return;
        }

        session.Status = HirePySessionStatus.TopCandidatesSelected;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await PublishAsync(session, HirePyEventNames.TopCandidatesSelected, ct, candidateCount: selected.Count);

        session.Status = HirePySessionStatus.InvitingCandidates;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var sent = 0;
        foreach (var candidate in selected)
        {
            var result = await _invitationService.CreateForClientAsync(
                new CreateProjectInvitationDto
                {
                    ProjectId = project.Id,
                    InviteeType = candidate.InviteeType,
                    InviteeUserId = candidate.InviteeType == ApplicantType.User ? candidate.InviteeId : null,
                    InviteeTeamId = candidate.InviteeType == ApplicantType.Team ? candidate.InviteeId : null,
                    Message = InvitationMessage
                },
                session.ClientUserId,
                ct);

            if (result.IsSuccess)
                sent++;
        }

        session.Status = HirePySessionStatus.WaitingForCandidates;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await PublishAsync(session, HirePyEventNames.InvitationsSent, ct, candidateCount: sent);
        await NotifyInvitationsSentAsync(session, sent, ct);
    }

    public async Task OnInvitationAcceptedAsync(Guid projectId, Guid proposalId, Guid freelancerUserId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByProjectIdAsync(projectId, ct);
        if (session is null)
            return;

        if (session.Status is HirePySessionStatus.Completed or HirePySessionStatus.Failed or HirePySessionStatus.Cancelled)
            return;

        session.Status = HirePySessionStatus.CandidateDiscussion;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await PublishAsync(session, HirePyEventNames.CandidateAccepted, ct);

        await _interviewService.EnsureInterviewAsync(session.Id, proposalId, freelancerUserId, ct);
    }

    public async Task OnInvitationDeclinedAsync(Guid projectId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByProjectIdAsync(projectId, ct);
        if (session is null)
            return;

        if (session.Status is HirePySessionStatus.Completed or HirePySessionStatus.Failed or HirePySessionStatus.Cancelled)
            return;

        await PublishAsync(session, HirePyEventNames.CandidateDeclined, ct);
    }

    private async Task<List<HirePySelectedCandidateDto>> SelectTopCandidatesAsync(
        Project project,
        IReadOnlyList<RankedProposal> ranked,
        List<ProjectProposal> proposals,
        IReadOnlyList<ProjectInvitation> existingInvitations,
        CancellationToken ct)
    {
        if (ranked.Count == 0 || proposals.Count == 0)
            return [];

        var proposalById = proposals.ToDictionary(p => p.Id);

        var candidates = new List<(RankedProposal Ranked, ProjectProposal Proposal, ApplicantType Type, Guid InviteeId)>();
        foreach (var rankedProposal in ranked)
        {
            if (!Guid.TryParse(rankedProposal.CandidateId, out var proposalId))
                continue;
            if (!proposalById.TryGetValue(proposalId, out var proposal))
                continue;

            var inviteeId = proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : proposal.UserId;
            if (!inviteeId.HasValue || inviteeId.Value == Guid.Empty)
                continue;

            candidates.Add((rankedProposal, proposal, proposal.ApplicantType, inviteeId.Value));
        }

        var userInviteeIds = candidates.Where(c => c.Type == ApplicantType.User).Select(c => c.InviteeId).ToHashSet();
        var teamInviteeIds = candidates.Where(c => c.Type == ApplicantType.Team).Select(c => c.InviteeId).ToHashSet();

        var users = await _userRepository.Query()
            .Where(u => userInviteeIds.Contains(u.Id))
            .Select(u => new { u.Id, u.IsDeleted })
            .ToListAsync(ct);
        var deletedUserIds = users.Where(u => u.IsDeleted).Select(u => u.Id).ToHashSet();

        var profiles = await _developerProfileRepository.Query()
            .Where(dp => userInviteeIds.Contains(dp.UserId))
            .Select(dp => new { dp.UserId, dp.IsDeleted })
            .ToListAsync(ct);
        var activeProfileUserIds = profiles.Where(p => !p.IsDeleted).Select(p => p.UserId).ToHashSet();

        var teams = await _teamRepository.Query()
            .Where(t => teamInviteeIds.Contains(t.Id))
            .Select(t => new { t.Id, t.IsDeleted })
            .ToListAsync(ct);
        var deletedTeamIds = teams.Where(t => t.IsDeleted).Select(t => t.Id).ToHashSet();

        var invitedKeys = existingInvitations
            .Where(i => i.Status != ProjectInvitationStatus.Cancelled)
            .Select(i => (Type: i.InviteeType, Id: i.InviteeType == ApplicantType.User ? i.InviteeUserId : i.InviteeTeamId))
            .Where(x => x.Id.HasValue)
            .Select(x => (x.Type, x.Id!.Value))
            .ToHashSet();

        var seen = new HashSet<(ApplicantType Type, Guid Id)>();
        var selected = new List<HirePySelectedCandidateDto>();

        foreach (var (rankedProposal, proposal, type, inviteeId) in candidates)
        {
            if (!seen.Add((type, inviteeId)))
                continue;

            if (invitedKeys.Contains((type, inviteeId)))
                continue;

            if (proposal.Status is not (ProposalStatus.Pending or ProposalStatus.Viewed))
                continue;

            if (type == ApplicantType.User)
            {
                if (deletedUserIds.Contains(inviteeId))
                    continue;
                if (!activeProfileUserIds.Contains(inviteeId))
                    continue;
                if (project.AssignedUserId == inviteeId)
                    continue;
            }
            else
            {
                if (deletedTeamIds.Contains(inviteeId))
                    continue;
                if (project.AssignedTeamId == inviteeId)
                    continue;
            }

            selected.Add(new HirePySelectedCandidateDto
            {
                ProposalId = proposal.Id,
                CandidateName = rankedProposal.CandidateName,
                Rank = rankedProposal.Rank,
                InviteeType = type,
                InviteeId = inviteeId
            });

            if (selected.Count >= TopCandidateCount)
                break;
        }

        return selected;
    }

    private async Task FailAsync(HirePySession session, string reason, CancellationToken ct)
    {
        session.Status = HirePySessionStatus.Failed;
        session.FailReason = reason;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await PublishAsync(session, HirePyEventNames.RankingFailed, ct,
            stage: HirePySessionStatus.Failed.ToString(),
            message: reason);
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

    private async Task NotifyNoCandidatesAsync(HirePySession session, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "No eligible candidates found",
            Body = "HirePy couldn't find eligible candidates for your project yet. Try again later or adjust your project details.",
            Type = NotificationType.HirePyNoCandidatesFound,
            ProjectId = session.ProjectId,
            ActionUrl = session.ProjectId.HasValue ? $"/client/projects/{session.ProjectId}" : "/client/hire-talent",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task NotifyInvitationsSentAsync(HirePySession session, int sentCount, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Invitations sent",
            Body = $"HirePy ranked the candidates and sent {sentCount} invitation(s) for your project.",
            Type = NotificationType.HirePyInvitationsSent,
            ProjectId = session.ProjectId,
            ActionUrl = session.ProjectId.HasValue ? $"/client/projects/{session.ProjectId}?tab=invitations" : "/client/hire-talent",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task NotifyFailedAsync(HirePySession session, CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "HirePy candidate matching failed",
            Body = "HirePy couldn't complete candidate matching for your project. Please try again.",
            Type = NotificationType.HirePyFailed,
            ProjectId = session.ProjectId,
            ActionUrl = session.ProjectId.HasValue ? $"/client/projects/{session.ProjectId}" : "/client/hire-talent",
            Data = JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private static string CoarseStatus(HirePySessionStatus status)
        => status switch
        {
            HirePySessionStatus.Completed => "Completed",
            HirePySessionStatus.Failed => "Failed",
            HirePySessionStatus.Cancelled => "Cancelled",
            _ => "Processing"
        };
}
