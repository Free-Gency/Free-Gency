using System.Text.Json;
using FreeGency.AI.HiringAgent;
using FreeGency.Application.Features.HiringAgent.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.Application.Features.HiringAgent;

public sealed class HiringAgentService : IHiringAgentService
{
    private const int MaxAgentMessages = 8;
    private const int TranscriptMessageLimit = 30;
    private static readonly TimeSpan DiscussionNudgeDelay = TimeSpan.FromMinutes(10);

    private static readonly HiringAgentRunStatus[] ActiveRunStatuses =
    [
        HiringAgentRunStatus.Queued,
        HiringAgentRunStatus.Inviting,
        HiringAgentRunStatus.WaitingAccepts,
        HiringAgentRunStatus.Discussing,
        HiringAgentRunStatus.Ranking
    ];

    private static readonly JsonSerializerOptions ReportJsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntitlementService _entitlementService;
    private readonly ISuggestionService _suggestionService;
    private readonly INotificationService _notificationService;
    private readonly IMilestoneService _milestoneService;
    private readonly DiscussionAgentChatService _discussionAgent;
    private readonly DiscussionRankingChatService _discussionRanking;
    private readonly IChatService _chatService;
    private readonly ILogger<HiringAgentService> _logger;
    private readonly HiringAgentOptions _options;

    private readonly IHiringAgentRunRepository _runRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMilestonePlanVersionRepository _planVersionRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IDeveloperProfileRepository _developerProfileRepository;
    private readonly IProjectProposalRepository _proposalRepository;

    public HiringAgentService(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IEntitlementService entitlementService,
        ISuggestionService suggestionService,
        INotificationService notificationService,
        IMilestoneService milestoneService,
        DiscussionAgentChatService discussionAgent,
        DiscussionRankingChatService discussionRanking,
        IChatService chatService,
        ILogger<HiringAgentService> logger,
        IOptions<HiringAgentOptions> hiringAgentOptions)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _entitlementService = entitlementService;
        _suggestionService = suggestionService;
        _notificationService = notificationService;
        _milestoneService = milestoneService;
        _discussionAgent = discussionAgent;
        _discussionRanking = discussionRanking;
        _chatService = chatService;
        _logger = logger;
        _options = hiringAgentOptions.Value;

        _runRepository = unitOfWork.Repository<IHiringAgentRunRepository, HiringAgentRun>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _invitationRepository = unitOfWork.Repository<IProjectInvitationRepository, ProjectInvitation>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _messageRepository = unitOfWork.Repository<IMessageRepository, Message>();
        _planVersionRepository = unitOfWork.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>();
        _chatRoomRepository = unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        _teamMemberRepository = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _teamRepository = unitOfWork.Repository<ITeamRepository, Team>();
        _developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
    }

    public async Task<ApiResponse<HiringAgentRunDto>> StartAsync(
        StartHiringAgentRunRequestDto request,
        CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.NotFound(nameof(Project), request.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<HiringAgentRunDto>(
                AppError.Forbidden("Only the project's client can start the hiring agent."));

        if (project.Status is not (ProjectStatus.Draft or ProjectStatus.Open))
            return ApiResponse.Failure<HiringAgentRunDto>(
                AppError.Validation("Project must be Draft or Open to start the hiring agent."));

        if (project.Status == ProjectStatus.Draft)
        {
            project.Status = ProjectStatus.Open;
            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync(ct);
            BackgroundJob.Enqueue<ISuggestionService>(s => s.IndexProjectAsync(project.Id, CancellationToken.None));
        }

        if (await _runRepository.HasActiveRunForProjectAsync(project.Id, ct))
            return ApiResponse.Failure<HiringAgentRunDto>(
                AppError.Validation("An active hiring agent run already exists for this project."));

        var quota = await _entitlementService.CanConsumeAsync(_currentUser.UserId, FeatureType.HiringAgent, ct);
        if (!quota.IsAllowed)
            return ApiResponse.Failure<HiringAgentRunDto>(quota.ToAppError());

        await _entitlementService.ConsumeAsync(_currentUser.UserId, FeatureType.HiringAgent, ct);

        var inviteHours = Clamp(
            request.InviteWindowHours ?? _options.DefaultInviteWindowHours,
            _options.MinInviteWindowHours,
            _options.MaxInviteWindowHours);
        var discussionHours = Clamp(
            request.DiscussionWindowHours ?? _options.DefaultDiscussionWindowHours,
            _options.MinDiscussionWindowHours,
            _options.MaxDiscussionWindowHours);

        if (discussionHours < inviteHours)
        {
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.Validation(
                "Discussion window must be greater than or equal to the invite window."));
        }

        var now = DateTime.UtcNow;
        var topK = request.TopK is > 0 ? request.TopK.Value : 5;
        var run = new HiringAgentRun
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ClientUserId = _currentUser.UserId,
            Status = HiringAgentRunStatus.Queued,
            TopK = topK,
            InviteDeadlineUtc = now.AddHours(inviteHours),
            DiscussionDeadlineUtc = now.AddHours(discussionHours),
            CreatedAt = now,
            CreatedBy = _currentUser.UserId.ToString()
        };

        await _runRepository.AddAsync(run, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.ProcessMatchAndInviteAsync(run.Id, CancellationToken.None));

        BackgroundJob.Schedule<IHiringAgentService>(
            s => s.EvaluateRunProgressAsync(run.Id, CancellationToken.None),
            new DateTimeOffset(run.InviteDeadlineUtc, TimeSpan.Zero));

        BackgroundJob.Schedule<IHiringAgentService>(
            s => s.EvaluateRunProgressAsync(run.Id, CancellationToken.None),
            new DateTimeOffset(run.DiscussionDeadlineUtc, TimeSpan.Zero));

        run.Project = project;
        return ApiResponse.Success(MapRunDto(run), "Hiring agent started.");
    }

    public async Task ProcessMatchAndInviteAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
        {
            _logger.LogWarning("Hiring agent run {RunId} not found for match/invite.", runId);
            return;
        }

        if (run.Status is HiringAgentRunStatus.Cancelled or HiringAgentRunStatus.Failed)
            return;

        var project = run.Project ?? await _projectRepository.GetByIdAsync(run.ProjectId, ct);
        if (project is null)
        {
            await FailRunAsync(run, "Project was not found.", ct);
            return;
        }

        run.Status = HiringAgentRunStatus.Inviting;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (run.Candidates.Any(c =>
                c.Status is HiringAgentCandidateStatus.Invited
                    or HiringAgentCandidateStatus.Accepted
                    or HiringAgentCandidateStatus.Discussing
                    or HiringAgentCandidateStatus.PlanProposed))
        {
            await EnsureExistingDiscussionsAttachedAsync(run, ct);
            return;
        }

        var clientUserId = run.ClientUserId;
        var projectId = run.ProjectId;
        var topK = run.TopK;
        var projectTitle = project.Title;

        // Suggestions + per-invite saves must not share a polluted change tracker with candidate inserts.
        _unitOfWork.ClearChangeTracker();

        var existingProposals = (await _proposalRepository.GetByProjectIdAsync(projectId, ct: ct))
            .Where(p => p.Status is ProposalStatus.Pending or ProposalStatus.Viewed or ProposalStatus.InDiscussion)
            .ToList();

        var existingByKey = existingProposals
            .Select(p => (
                Key: ApplicantKey(p.ApplicantType, p.ApplicantType == ApplicantType.Team ? p.TeamId : p.UserId),
                Proposal: p))
            .Where(x => x.Key is not null)
            .GroupBy(x => x.Key!)
            .ToDictionary(g => g.Key, g => g.First().Proposal);

        // Pull extra matches so we can still fill TopK invites after skipping applicants.
        var suggest = await _suggestionService.SuggestCandidatesForProjectAsSystemAsync(
            projectId,
            clientUserId,
            Math.Clamp(topK * 3, topK, 30),
            ct);

        if (!suggest.IsSuccess || suggest.Data is null)
        {
            var reason = suggest.Error?.message ?? "Candidate matching failed.";
            run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct)
                  ?? throw new InvalidOperationException($"Hiring agent run {runId} disappeared.");
            // Still OK to continue with existing discussions only.
            if (existingProposals.Count == 0)
            {
                await FailRunAsync(run, reason, ct);
                return;
            }
        }

        var built = new List<HiringAgentCandidate>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var rank = 0;
        var inviteSlots = topK;
        var discussionKickoffIds = new List<Guid>();

        // 1) Keep working existing open discussions on this project.
        foreach (var proposal in existingProposals.Where(p => p.Status == ProposalStatus.InDiscussion))
        {
            var key = ApplicantKey(
                proposal.ApplicantType,
                proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : proposal.UserId);
            if (key is null || !seenKeys.Add(key)) continue;

            rank++;
            var room = await _chatRoomRepository.GetByProposalIdAsync(proposal.Id, ct);
            var (name, avatar) = await ResolveApplicantDisplayAsync(proposal, ct);
            var matchedScore = suggest.Data?.Candidates
                .FirstOrDefault(s => MatchesSuggestion(s, proposal))
                ?.FinalScore ?? 0f;

            var candidate = new HiringAgentCandidate
            {
                Id = Guid.NewGuid(),
                HiringAgentRunId = runId,
                InviteeType = proposal.ApplicantType,
                InviteeUserId = proposal.ApplicantType == ApplicantType.User ? proposal.UserId : null,
                InviteeTeamId = proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : null,
                DisplayName = name,
                AvatarUrl = avatar,
                SuggestionScore = matchedScore,
                RankOrder = rank,
                Status = HiringAgentCandidateStatus.Discussing,
                ProposalId = proposal.Id,
                ChatRoomId = room?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = clientUserId.ToString()
            };
            built.Add(candidate);
            if (room is not null)
                discussionKickoffIds.Add(candidate.Id);
        }

        // 2) AI recommendations
        foreach (var suggested in suggest.Data?.Candidates ?? [])
        {
            var isTeam = string.Equals(suggested.CandidateType, "Team", StringComparison.OrdinalIgnoreCase);
            var inviteeUserId = isTeam ? (Guid?)null : suggested.Id;
            var inviteeTeamId = isTeam ? suggested.Id : (Guid?)null;
            var key = ApplicantKey(
                isTeam ? ApplicantType.Team : ApplicantType.User,
                isTeam ? inviteeTeamId : inviteeUserId);
            if (key is null) continue;

            if (seenKeys.Contains(key))
            {
                // Already attached as an open discussion — bump AI score if we had 0.
                var existing = built.FirstOrDefault(c =>
                    ApplicantKey(c.InviteeType, c.InviteeType == ApplicantType.Team ? c.InviteeTeamId : c.InviteeUserId) == key);
                if (existing is not null && existing.SuggestionScore <= 0)
                    existing.SuggestionScore = suggested.FinalScore;
                continue;
            }

            existingByKey.TryGetValue(key, out var existingProposal);

            if (existingProposal is not null)
            {
                // Already applied — never invite. Attach and (if needed) continue via discussion room.
                seenKeys.Add(key);
                rank++;
                _unitOfWork.ClearChangeTracker();
                var room = await _chatRoomRepository.GetByProposalIdAsync(existingProposal.Id, ct);
                var candidate = new HiringAgentCandidate
                {
                    Id = Guid.NewGuid(),
                    HiringAgentRunId = runId,
                    InviteeType = existingProposal.ApplicantType,
                    InviteeUserId = existingProposal.ApplicantType == ApplicantType.User ? existingProposal.UserId : null,
                    InviteeTeamId = existingProposal.ApplicantType == ApplicantType.Team ? existingProposal.TeamId : null,
                    DisplayName = suggested.Name,
                    AvatarUrl = suggested.AvatarUrl,
                    SuggestionScore = suggested.FinalScore,
                    RankOrder = rank,
                    Status = room is not null || existingProposal.Status == ProposalStatus.InDiscussion
                        ? HiringAgentCandidateStatus.Discussing
                        : HiringAgentCandidateStatus.Accepted,
                    ProposalId = existingProposal.Id,
                    ChatRoomId = room?.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = clientUserId.ToString()
                };
                built.Add(candidate);
                if (candidate.Status == HiringAgentCandidateStatus.Discussing && room is not null)
                    discussionKickoffIds.Add(candidate.Id);
                continue;
            }

            if (inviteSlots <= 0)
                continue;

            seenKeys.Add(key);
            rank++;
            var inviteCandidate = new HiringAgentCandidate
            {
                Id = Guid.NewGuid(),
                HiringAgentRunId = runId,
                InviteeType = isTeam ? ApplicantType.Team : ApplicantType.User,
                InviteeUserId = inviteeUserId,
                InviteeTeamId = inviteeTeamId,
                DisplayName = suggested.Name,
                AvatarUrl = suggested.AvatarUrl,
                SuggestionScore = suggested.FinalScore,
                RankOrder = rank,
                Status = HiringAgentCandidateStatus.Suggested,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = clientUserId.ToString()
            };

            _unitOfWork.ClearChangeTracker();
            var inviteResult = await CreateHiringInviteAsync(
                runId,
                projectId,
                clientUserId,
                projectTitle,
                inviteCandidate,
                ct);

            if (inviteResult is null)
            {
                inviteCandidate.Status = HiringAgentCandidateStatus.InviteFailed;
                _logger.LogWarning(
                    "Hiring agent run {RunId}: invite failed for {CandidateType} {InviteeId} ({Name})",
                    runId,
                    inviteCandidate.InviteeType,
                    inviteeUserId ?? inviteeTeamId,
                    inviteCandidate.DisplayName);
            }
            else
            {
                inviteCandidate.InvitationId = inviteResult.Value;
                inviteCandidate.Status = HiringAgentCandidateStatus.Invited;
                inviteSlots--;
            }

            built.Add(inviteCandidate);
        }

        var usable = built.Where(c => c.Status != HiringAgentCandidateStatus.InviteFailed).ToList();
        if (usable.Count == 0)
        {
            _unitOfWork.ClearChangeTracker();
            run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
            if (run is null) return;
            await FailRunAsync(run, "Could not invite or attach any candidates.", ct);
            return;
        }

        await _runRepository.PersistMatchInviteResultsAsync(runId, built, ct);

        foreach (var candidateId in discussionKickoffIds)
        {
            BackgroundJob.Enqueue<IHiringAgentService>(s =>
                s.ProcessDiscussionTurnAsync(candidateId, CancellationToken.None));
        }
    }

    private static string? ApplicantKey(ApplicantType type, Guid? id)
        => id is null || id == Guid.Empty ? null : $"{type}:{id.Value:N}";

    private static bool MatchesSuggestion(
        FreeGency.Application.Features.Suggestions.DTOs.SuggestedCandidateDto suggested,
        ProjectProposal proposal)
    {
        var isTeam = string.Equals(suggested.CandidateType, "Team", StringComparison.OrdinalIgnoreCase);
        if (isTeam)
            return proposal.ApplicantType == ApplicantType.Team && proposal.TeamId == suggested.Id;
        return proposal.ApplicantType == ApplicantType.User && proposal.UserId == suggested.Id;
    }

    private async Task<(string Name, string? Avatar)> ResolveApplicantDisplayAsync(
        ProjectProposal proposal,
        CancellationToken ct)
    {
        if (proposal.ApplicantType == ApplicantType.Team && proposal.TeamId is Guid teamId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, ct);
            return (team?.Name ?? "Team", team?.Logo);
        }

        if (proposal.UserId is Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            var name = user is null
                ? "Developer"
                : $"{user.FristName} {user.LastName}".Trim();
            var avatar = user?.DeveloperProfile?.ProfileImage;
            return (string.IsNullOrWhiteSpace(name) ? "Developer" : name, avatar);
        }

        return ("Applicant", null);
    }

    public async Task OnInvitationAcceptedAsync(
        Guid invitationId,
        Guid proposalId,
        Guid chatRoomId,
        CancellationToken ct = default)
    {
        var run = await _runRepository.GetByInvitationIdAsync(invitationId, ct);
        if (run is null)
            return;

        var candidate = run.Candidates.FirstOrDefault(c => c.InvitationId == invitationId);
        if (candidate is null)
            return;

        candidate.ProposalId = proposalId;
        candidate.ChatRoomId = chatRoomId;
        candidate.Status = HiringAgentCandidateStatus.Discussing;
        candidate.UpdatedAt = DateTime.UtcNow;

        if (run.Status is HiringAgentRunStatus.WaitingAccepts or HiringAgentRunStatus.Inviting
            or HiringAgentRunStatus.Queued)
        {
            run.Status = HiringAgentRunStatus.Discussing;
        }

        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.ProcessDiscussionTurnAsync(candidate.Id, CancellationToken.None));
    }

    public async Task OnFreelancerMessageAsync(Guid chatRoomId, CancellationToken ct = default)
    {
        var candidate = await _runRepository.GetCandidateByChatRoomIdAsync(chatRoomId, ct);
        if (candidate is null)
            return;

        var run = candidate.HiringAgentRun;
        if (run is null || !ActiveRunStatuses.Contains(run.Status))
            return;

        // Keep chatting through WaitingAccepts + Discussing so freelancers get answers.
        if (run.Status is not (HiringAgentRunStatus.WaitingAccepts or HiringAgentRunStatus.Discussing))
            return;

        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.ProcessDiscussionTurnAsync(candidate.Id, CancellationToken.None));
    }

    public async Task OnInvitationRejectedAsync(Guid invitationId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByInvitationIdAsync(invitationId, ct);
        if (run is null)
            return;

        var candidate = run.Candidates.FirstOrDefault(c => c.InvitationId == invitationId);
        if (candidate is null)
            return;

        candidate.Status = HiringAgentCandidateStatus.Rejected;
        candidate.UpdatedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.EvaluateRunProgressAsync(run.Id, CancellationToken.None));
    }

    public async Task ProcessDiscussionTurnAsync(Guid candidateId, CancellationToken ct = default)
    {
        // Tracked load — Query() is AsNoTracking and would silently drop AgentMessageCount updates.
        var run = await _runRepository.GetByCandidateIdWithDetailsAsync(candidateId, ct);
        if (run is null)
            return;

        var candidate = run.Candidates.FirstOrDefault(c => c.Id == candidateId);
        if (candidate is null || candidate.ChatRoomId is null || candidate.ProposalId is null)
            return;

        if (run.Status is not (HiringAgentRunStatus.Discussing or HiringAgentRunStatus.WaitingAccepts))
            return;

        if (candidate.AgentMessageCount >= MaxAgentMessages)
        {
            BackgroundJob.Enqueue<IHiringAgentService>(s =>
                s.EvaluateRunProgressAsync(run.Id, CancellationToken.None));
            return;
        }

        var project = run.Project ?? await _projectRepository.GetByIdAsync(run.ProjectId, ct);
        if (project is null)
            return;

        var recent = (await _messageRepository.GetLatestAsync(candidate.ChatRoomId.Value, TranscriptMessageLimit, ct))
            .OrderBy(m => m.CreatedAt)
            .ToList();

        var lastBeforeReply = recent.LastOrDefault();
        var lastFromFreelancer = lastBeforeReply?.SenderDeveloperProfileId is not null;
        var lastIsOurs = lastBeforeReply?.IsAgentGenerated == true
            || (lastBeforeReply?.SenderClientProfileId is not null
                && lastBeforeReply.SenderDeveloperProfileId is null);

        // Don't spam another agent message while waiting for the freelancer.
        if (!lastFromFreelancer && lastIsOurs && candidate.AgentMessageCount > 0)
        {
            BackgroundJob.Schedule<IHiringAgentService>(
                s => s.ProcessDiscussionTurnAsync(candidate.Id, CancellationToken.None),
                DiscussionNudgeDelay);
            return;
        }

        var latestPlan = await _planVersionRepository.Query()
            .Where(p => p.ProposalId == candidate.ProposalId.Value && !p.IsDeleted)
            .OrderByDescending(p => p.Version)
            .ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        MilestonePlanVersion? planWithItems = null;
        if (latestPlan is not null)
            planWithItems = await _planVersionRepository.GetByIdWithItemsAsync(latestPlan.Id, ct) ?? latestPlan;

        var hasPlan = planWithItems is not null;
        var planStatus = planWithItems?.Status.ToString() ?? "None";
        var planSummary = BuildPlanSummary(planWithItems);
        var projectContext = BuildProjectContext(project);
        var transcript = BuildTranscript(recent);
        var reply = await _discussionAgent.GenerateReplyAsync(
            projectContext,
            candidate.DisplayName,
            transcript,
            hasPlan,
            planStatus,
            planSummary,
            ct);

        // Never ask for plan revisions in chat before the client selects this candidate.
        if (hasPlan && planWithItems!.Status == PlanVersionStatus.Proposed)
        {
            if (LooksLikePlanRevisionAsk(reply.Message) || string.IsNullOrWhiteSpace(reply.Message))
            {
                reply = new DiscussionAgentReply
                {
                    Message =
                        "Thanks — I've received your milestone plan in the product. I'll include it when we wrap discussions; any refinements happen only if the client chooses to move forward with you.",
                    RequestPlanChanges = false,
                    ChangeComment = null,
                    DoneNegotiating = true,
                    InternalNote = string.IsNullOrWhiteSpace(reply.InternalNote)
                        ? "Plan received; deferred critique until client selection."
                        : reply.InternalNote
                };
            }
            else
            {
                reply = new DiscussionAgentReply
                {
                    Message = reply.Message,
                    RequestPlanChanges = false,
                    ChangeComment = null,
                    DoneNegotiating = true,
                    InternalNote = reply.InternalNote
                };
            }
        }
        else if (LooksLikePlanRevisionAsk(reply.Message))
        {
            reply = new DiscussionAgentReply
            {
                Message =
                    "Thanks — please keep using the product milestone plan when you're ready. I won't request formal changes until the client selects a candidate.",
                RequestPlanChanges = false,
                ChangeComment = null,
                DoneNegotiating = hasPlan,
                InternalNote = reply.InternalNote
            };
        }

        if (string.IsNullOrWhiteSpace(reply.Message))
            return;

        var send = await _chatService.SendAsClientAsync(
            candidate.ChatRoomId.Value,
            run.ClientUserId,
            reply.Message,
            isAgentGenerated: true);

        if (!send.IsSuccess)
        {
            _logger.LogWarning(
                "Hiring agent failed to send discussion message for candidate {CandidateId}: {Error}",
                candidateId,
                send.error.Discription);
            return;
        }

        // Formal change-requests are deferred until the client confirms the recommended candidate.
        candidate.AgentMessageCount++;
        candidate.LastAgentMessageAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reply.InternalNote))
        {
            candidate.DiscussionNotes = string.IsNullOrWhiteSpace(candidate.DiscussionNotes)
                ? reply.InternalNote
                : $"{candidate.DiscussionNotes}\n{reply.InternalNote}";
        }

        if (planWithItems is not null)
        {
            candidate.LatestPlanVersionId = planWithItems.Id;
            candidate.Status = HiringAgentCandidateStatus.PlanProposed;
        }
        else if (candidate.Status != HiringAgentCandidateStatus.PlanProposed)
        {
            candidate.Status = HiringAgentCandidateStatus.Discussing;
        }

        if (run.Status == HiringAgentRunStatus.WaitingAccepts)
            run.Status = HiringAgentRunStatus.Discussing;

        candidate.UpdatedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var done = reply.DoneNegotiating || candidate.AgentMessageCount >= MaxAgentMessages;
        if (done)
        {
            BackgroundJob.Enqueue<IHiringAgentService>(s =>
                s.EvaluateRunProgressAsync(run.Id, CancellationToken.None));
            return;
        }

        // Nudge only while waiting on the freelancer (not right after answering them).
        if (!lastFromFreelancer)
        {
            BackgroundJob.Schedule<IHiringAgentService>(
                s => s.ProcessDiscussionTurnAsync(candidate.Id, CancellationToken.None),
                DiscussionNudgeDelay);
        }
    }

    public async Task EvaluateRunProgressAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return;

        if (run.Status is HiringAgentRunStatus.ReportReady
            or HiringAgentRunStatus.Hired
            or HiringAgentRunStatus.Cancelled
            or HiringAgentRunStatus.Failed
            or HiringAgentRunStatus.Dismissed
            or HiringAgentRunStatus.Ranking)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var inviteDeadlinePassed = now >= run.InviteDeadlineUtc;
        var discussionDeadlinePassed = now >= run.DiscussionDeadlineUtc;

        static List<HiringAgentCandidate> DiscussionCandidates(HiringAgentRun r) =>
            r.Candidates
                .Where(c => c.ChatRoomId is not null
                            && c.Status is HiringAgentCandidateStatus.Discussing
                                or HiringAgentCandidateStatus.PlanProposed
                                or HiringAgentCandidateStatus.Accepted)
                .ToList();

        var discussionCandidates = DiscussionCandidates(run);
        var pendingInvites = run.Candidates.Any(c => c.Status == HiringAgentCandidateStatus.Invited);

        // Keep unanswered invites Pending until the invite window actually ends.
        // Do NOT auto-expire just because enough acceptors already have plans —
        // that made "Invite expired — no reply" appear while discussion was still open.
        // Clients can still close early via CloseInvitesAsync ("Finish early & Rank").
        if (!inviteDeadlinePassed)
        {
            var reopened = await ReopenPrematurelyExpiredInvitesAsync(run, now, ct);
            if (reopened > 0)
                pendingInvites = run.Candidates.Any(c => c.Status == HiringAgentCandidateStatus.Invited);
        }

        if (pendingInvites && inviteDeadlinePassed)
        {
            var expired = await ExpirePendingInvitesAsync(run, now, ct);
            pendingInvites = false;
            if (expired > 0)
            {
                await NotifyClientAsync(
                    run.ClientUserId,
                    "Invites closed",
                    expired == 1
                        ? $"1 pending invite was closed. Continuing with {discussionCandidates.Count} candidate(s)."
                        : $"{expired} pending invites were closed. Continuing with {discussionCandidates.Count} candidate(s).",
                    NotificationType.HiringAgentReportReady,
                    run.ProjectId,
                    $"/client/reports/hiring-agent/{run.Id}",
                    ct);
            }
        }

        if (pendingInvites && !inviteDeadlinePassed)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        discussionCandidates = DiscussionCandidates(run);
        if (discussionCandidates.Count == 0)
        {
            await FailRunAsync(run, "No candidates accepted invitations before the deadline.", ct);
            return;
        }

        if (run.Status == HiringAgentRunStatus.WaitingAccepts)
            run.Status = HiringAgentRunStatus.Discussing;

        // Ranking waits for the discussion window OR an explicit Finish early
        // (CloseInvitesAsync). Do NOT auto-rank just because every candidate
        // already proposed a plan / agent marked doneNegotiating.
        if (discussionDeadlinePassed)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            BackgroundJob.Enqueue<IHiringAgentService>(s =>
                s.ProcessRankingAsync(run.Id, CancellationToken.None));
            return;
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<ApiResponse<HiringAgentRunDto>> CloseInvitesAsync(
        Guid runId,
        CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.NotFound(nameof(HiringAgentRun), runId));

        if (run.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure<HiringAgentRunDto>(
                AppError.Forbidden("You do not own this hiring agent run."));

        if (run.Status is not (HiringAgentRunStatus.WaitingAccepts or HiringAgentRunStatus.Discussing))
        {
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.Validation(
                "You can only finish early while the run is waiting for accepts or discussing."));
        }

        var now = DateTime.UtcNow;
        var expired = await ExpirePendingInvitesAsync(run, now, ct);

        var accepted = run.Candidates
            .Where(c => c.ChatRoomId is not null
                        && c.Status is HiringAgentCandidateStatus.Discussing
                            or HiringAgentCandidateStatus.PlanProposed
                            or HiringAgentCandidateStatus.Accepted)
            .ToList();

        if (accepted.Count == 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.Validation(
                "No one has accepted yet. Wait for at least one acceptance before finishing early."));
        }

        // Client chose not to wait — close windows and rank whoever already accepted.
        if (run.InviteDeadlineUtc > now)
            run.InviteDeadlineUtc = now;
        if (run.DiscussionDeadlineUtc > now)
            run.DiscussionDeadlineUtc = now;

        run.Status = HiringAgentRunStatus.Ranking;
        run.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(ct);

        await NotifyClientAsync(
            run.ClientUserId,
            "Finishing Hire by AI early",
            expired > 0
                ? $"Pending invites closed. Ranking {accepted.Count} candidate(s) who already accepted."
                : $"Ranking {accepted.Count} candidate(s) who already accepted.",
            NotificationType.HiringAgentReportReady,
            run.ProjectId,
            $"/client/reports/hiring-agent/{run.Id}",
            ct);

        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.ProcessRankingAsync(run.Id, CancellationToken.None));

        run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct) ?? run;
        return ApiResponse.Success(
            MapRunDto(run),
            $"Finishing early with {accepted.Count} candidate(s). Ranking now.");
    }

    public async Task ProcessRankingAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return;

        if (run.Status is HiringAgentRunStatus.ReportReady
            or HiringAgentRunStatus.Hired
            or HiringAgentRunStatus.Cancelled
            or HiringAgentRunStatus.Failed
            or HiringAgentRunStatus.Dismissed)
        {
            return;
        }

        var project = run.Project ?? await _projectRepository.GetByIdAsync(run.ProjectId, ct);
        if (project is null)
        {
            await FailRunAsync(run, "Project was not found during ranking.", ct);
            return;
        }

        run.Status = HiringAgentRunStatus.Ranking;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var rankable = run.Candidates
            .Where(c => c.ChatRoomId is not null)
            .OrderBy(c => c.RankOrder)
            .ToList();

        if (rankable.Count == 0)
        {
            await FailRunAsync(run, "No discussion transcripts available to rank.", ct);
            return;
        }

        var candidatesBlock = new System.Text.StringBuilder();
        foreach (var candidate in rankable)
        {
            var messages = (await _messageRepository.GetLatestAsync(candidate.ChatRoomId!.Value, 50, ct))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            string planSummary = "(no milestone plan)";
            MilestonePlanVersion? plan = null;
            if (candidate.LatestPlanVersionId is Guid planId)
            {
                plan = await _planVersionRepository.GetByIdWithItemsAsync(planId, ct);
            }
            else if (candidate.ProposalId is Guid proposalId)
            {
                var latest = await _planVersionRepository.Query()
                    .Where(p => p.ProposalId == proposalId && !p.IsDeleted)
                    .OrderByDescending(p => p.Version)
                    .ThenByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync(ct);
                if (latest is not null)
                    plan = await _planVersionRepository.GetByIdWithItemsAsync(latest.Id, ct);
            }

            if (plan is not null)
            {
                candidate.LatestPlanVersionId ??= plan.Id;
                planSummary = string.Join(
                    "; ",
                    plan.Items.OrderBy(i => i.SortOrder)
                        .Select(i => $"{i.Title}: {i.Amount:0.##} due {i.DueDate:yyyy-MM-dd}"));
            }

            candidatesBlock.AppendLine($"--- CANDIDATE_ID={candidate.Id} NAME={candidate.DisplayName} ---");
            candidatesBlock.AppendLine($"PLAN: {planSummary}");
            candidatesBlock.AppendLine("TRANSCRIPT:");
            candidatesBlock.AppendLine(BuildTranscript(messages));
            candidatesBlock.AppendLine();
        }

        var ranking = await _discussionRanking.RankAsync(
            BuildProjectContext(project),
            candidatesBlock.ToString(),
            ct);

        var scoreById = new Dictionary<Guid, RankedDiscussionItem>();
        foreach (var item in ranking.Ranked)
        {
            if (Guid.TryParse(item.CandidateId, out var id))
                scoreById[id] = item;
        }

        var rankedDtos = new List<HiringAgentRankedDiscussionDto>();
        var rankOrder = 0;
        foreach (var candidate in rankable
                     .OrderByDescending(c => scoreById.TryGetValue(c.Id, out var r) ? r.Score : c.SuggestionScore)
                     .ThenByDescending(c => c.LatestPlanVersionId.HasValue)
                     .ThenBy(c => c.RankOrder))
        {
            rankOrder++;
            scoreById.TryGetValue(candidate.Id, out var ranked);
            var score = ranked?.Score ?? candidate.SuggestionScore;
            var summary = ranked?.Summary ?? candidate.DiscussionNotes ?? string.Empty;

            candidate.DiscussionScore = score;
            candidate.DiscussionNotes = summary;
            candidate.Status = HiringAgentCandidateStatus.Ranked;
            candidate.UpdatedAt = DateTime.UtcNow;

            rankedDtos.Add(new HiringAgentRankedDiscussionDto
            {
                CandidateId = candidate.Id,
                DisplayName = candidate.DisplayName,
                Score = score,
                Rank = rankOrder,
                Summary = summary,
                Strengths = ranked?.Strengths ?? [],
                Weaknesses = ranked?.Weaknesses ?? [],
                ChatRoomId = candidate.ChatRoomId,
                PlanVersionId = candidate.LatestPlanVersionId,
                HasMilestonePlan = candidate.LatestPlanVersionId.HasValue
            });
        }

        var recommended = rankable
            .OrderByDescending(c => c.LatestPlanVersionId.HasValue)
            .ThenByDescending(c => c.DiscussionScore ?? 0)
            .ThenBy(c => c.RankOrder)
            .First();

        run.RecommendedCandidateId = recommended.Id;
        run.RecommendedProposalId = recommended.ProposalId;
        run.RecommendedPlanVersionId = recommended.LatestPlanVersionId;
        run.Status = HiringAgentRunStatus.ReportReady;
        run.ReportReadyAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        run.FailureReason = null;

        var report = new HiringAgentReportDto
        {
            RunId = run.Id,
            ProjectId = run.ProjectId,
            ProjectTitle = project.Title,
            Status = HiringAgentRunStatus.ReportReady,
            Summary = ranking.OverallSummary,
            Recommended = MapCandidateDto(recommended),
            RecommendedPlanVersionId = run.RecommendedPlanVersionId,
            RankedDiscussions = rankedDtos,
            Risks = ranking.Risks,
            ReportReadyAt = run.ReportReadyAt
        };

        run.ReportJson = JsonSerializer.Serialize(report, ReportJsonOpts);
        await _unitOfWork.SaveChangesAsync(ct);

        await NotifyClientAsync(
            run.ClientUserId,
            "Hiring agent report ready",
            $"Your hiring agent finished evaluating candidates for \"{project.Title}\".",
            NotificationType.HiringAgentReportReady,
            run.ProjectId,
            $"/client/reports/hiring-agent/{run.Id}",
            ct);
    }

    public async Task<ApiResponse<HiringAgentRunDto>> GetRunAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.NotFound(nameof(HiringAgentRun), runId));

        if (run.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.Forbidden("You do not own this hiring agent run."));

        await TryRepairOpenInviteWindowAsync(run, ct);
        await EnsureExistingDiscussionsAttachedAsync(run, ct);
        return ApiResponse.Success(MapRunDto(run));
    }

    public async Task<ApiResponse<HiringAgentRunDto>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.NotFound(nameof(Project), projectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<HiringAgentRunDto>(AppError.Forbidden("You do not own this project."));

        var run = await _runRepository.GetActiveByProjectIdAsync(projectId, ct);
        if (run is null)
        {
            run = (await _runRepository.Query()
                    .AsSplitQuery()
                    .Include(r => r.Candidates)
                    .Include(r => r.Project)
                    .Where(r => r.ProjectId == projectId && r.ClientUserId == _currentUser.UserId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync(ct));
        }

        if (run is null)
            return ApiResponse.Failure<HiringAgentRunDto>(
                AppError.NotFound(nameof(HiringAgentRun), projectId));

        await TryRepairOpenInviteWindowAsync(run, ct);
        await EnsureExistingDiscussionsAttachedAsync(run, ct);
        return ApiResponse.Success(MapRunDto(run));
    }

    public async Task<ApiResponse<IReadOnlyList<HiringAgentRunDto>>> ListMineAsync(CancellationToken ct = default)
    {
        var runs = await _runRepository.GetByClientAsync(_currentUser.UserId, ct);
        return ApiResponse.Success<IReadOnlyList<HiringAgentRunDto>>(runs.Select(MapRunDto).ToList());
    }

    public async Task<ApiResponse<HiringAgentReportDto>> GetReportAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return ApiResponse.Failure<HiringAgentReportDto>(AppError.NotFound(nameof(HiringAgentRun), runId));

        if (run.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure<HiringAgentReportDto>(AppError.Forbidden("You do not own this hiring agent run."));

        if (string.IsNullOrWhiteSpace(run.ReportJson))
            return ApiResponse.Failure<HiringAgentReportDto>(
                AppError.Validation("Hiring agent report is not ready yet."));

        try
        {
            var report = JsonSerializer.Deserialize<HiringAgentReportDto>(run.ReportJson, ReportJsonOpts);
            if (report is null)
                return ApiResponse.Failure<HiringAgentReportDto>(
                    AppError.Validation("Hiring agent report could not be read."));

            return ApiResponse.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize hiring agent report for run {RunId}", runId);
            return ApiResponse.Failure<HiringAgentReportDto>(
                AppError.Validation("Hiring agent report could not be read."));
        }
    }

    public async Task<ApiResponse> ConfirmHireAsync(
        Guid runId,
        Guid? candidateId = null,
        CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(HiringAgentRun), runId));

        if (run.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("You do not own this hiring agent run."));

        if (run.Status != HiringAgentRunStatus.ReportReady)
            return ApiResponse.Failure(AppError.Validation("Hiring agent report must be ready before confirming hire."));

        HiringAgentCandidate? selected;
        if (candidateId is Guid chosenId)
        {
            selected = run.Candidates.FirstOrDefault(c => c.Id == chosenId);
            if (selected is null)
                return ApiResponse.Failure(AppError.Validation("Selected candidate was not found on this run."));
            if (selected.ProposalId is null)
                return ApiResponse.Failure(AppError.Validation(
                    "Selected candidate has no discussion proposal to hire from."));
        }
        else
        {
            if (run.RecommendedCandidateId is null || run.RecommendedProposalId is null)
                return ApiResponse.Failure(AppError.Validation("No recommended candidate is available to hire."));

            selected = run.Candidates.FirstOrDefault(c => c.Id == run.RecommendedCandidateId.Value);
            if (selected is null)
                return ApiResponse.Failure(AppError.Validation("Recommended candidate was not found."));
            if (selected.ProposalId is null)
                return ApiResponse.Failure(AppError.Validation(
                    "Recommended candidate has no discussion proposal to hire from."));
        }

        // Client may override the AI recommendation — keep run pointers in sync.
        run.RecommendedCandidateId = selected.Id;
        run.RecommendedProposalId = selected.ProposalId;

        // Always use the latest plan for this proposal (covers revisions after a prior change-request).
        var latestPlan = await _planVersionRepository.Query()
            .Where(p => p.ProposalId == selected.ProposalId.Value && !p.IsDeleted)
            .OrderByDescending(p => p.Version)
            .ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (latestPlan is null)
            return ApiResponse.Failure(AppError.Validation(
                "No milestone plan is available for this candidate. They must propose a plan before hire."));

        var planWithItems = await _planVersionRepository.GetByIdWithItemsAsync(latestPlan.Id, ct) ?? latestPlan;
        run.RecommendedPlanVersionId = planWithItems.Id;
        selected.LatestPlanVersionId = planWithItems.Id;

        if (planWithItems.Status == PlanVersionStatus.ChangesRequested)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return ApiResponse.Failure(AppError.Validation(
                "Waiting for this freelancer to submit a revised milestone plan."));
        }

        if (planWithItems.Status != PlanVersionStatus.Proposed)
        {
            return ApiResponse.Failure(AppError.Validation(
                "The milestone plan must be in Proposed status before confirming hire."));
        }

        var project = run.Project ?? await _projectRepository.GetByIdAsync(run.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.Validation("Project was not found."));

        // Client approved this candidate — only now may the agent open a formal change-request.
        run.ClientHireApprovedAt ??= DateTime.UtcNow;

        var recent = selected.ChatRoomId is Guid roomId
            ? (await _messageRepository.GetLatestAsync(roomId, TranscriptMessageLimit, ct))
                .OrderBy(m => m.CreatedAt)
                .ToList()
            : [];

        var review = await _discussionAgent.GenerateFinalPlanReviewAsync(
            BuildProjectContext(project),
            selected.DisplayName,
            BuildTranscript(recent),
            hasMilestonePlan: true,
            planWithItems.Status.ToString(),
            BuildPlanSummary(planWithItems),
            ct);

        if (review.RequestPlanChanges)
        {
            var changeComment = string.IsNullOrWhiteSpace(review.ChangeComment)
                ? review.Message
                : review.ChangeComment!;

            if (string.IsNullOrWhiteSpace(changeComment))
            {
                return ApiResponse.Failure(AppError.Validation(
                    "Hiring agent could not produce a change-request comment for this plan."));
            }

            // Formal product Request Changes only — no extra informal chat critique.
            var changes = await _milestoneService.RequestPlanChangesAsClientAsync(
                planWithItems.Id,
                run.ClientUserId,
                changeComment,
                isAgentGenerated: true,
                ct);

            if (!changes.IsSuccess)
                return changes;

            selected.DiscussionNotes = string.IsNullOrWhiteSpace(selected.DiscussionNotes)
                ? "Final review: requested plan changes after client approval"
                : $"{selected.DiscussionNotes}\nFinal review: requested plan changes after client approval";
            selected.UpdatedAt = DateTime.UtcNow;
            run.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success(
                "You approved this candidate. The hiring agent will keep refining the milestone plan with them until it is ready — you do not need to click Hire again.");
        }

        var accept = await _milestoneService.AcceptPlanAsync(planWithItems.Id, ct);
        if (!accept.IsSuccess)
            return accept;

        run.Status = HiringAgentRunStatus.Hired;
        run.CompletedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success($"Hire confirmed for {selected.DisplayName}.");
    }

    public async Task ProcessPostHirePlanReviewAsync(
        Guid proposalId,
        Guid planVersionId,
        CancellationToken ct = default)
    {
        var run = await _runRepository.GetClientApprovedReportReadyByProposalIdAsync(proposalId, ct);

        // Recover mid-loop runs created before ClientHireApprovedAt existed.
        if (run is null)
        {
            run = await _runRepository.Query()
                .AsSplitQuery()
                .Include(r => r.Candidates)
                .Include(r => r.Project)
                .Where(r =>
                    r.Status == HiringAgentRunStatus.ReportReady
                    && r.RecommendedProposalId == proposalId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .FirstOrDefaultAsync(ct);

            var recovered = run?.Candidates.FirstOrDefault(c => c.ProposalId == proposalId);
            var notes = recovered?.DiscussionNotes ?? string.Empty;
            if (run is null
                || recovered is null
                || !notes.Contains("requested plan changes after client approval", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            run.ClientHireApprovedAt ??= DateTime.UtcNow;
            run.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }

        if (run is null || run.ClientHireApprovedAt is null)
            return;

        if (run.Status != HiringAgentRunStatus.ReportReady)
            return;

        if (run.RecommendedProposalId != proposalId)
            return;

        var selected = run.Candidates.FirstOrDefault(c => c.Id == run.RecommendedCandidateId)
            ?? run.Candidates.FirstOrDefault(c => c.ProposalId == proposalId);
        if (selected is null)
            return;

        var planWithItems = await _planVersionRepository.GetByIdWithItemsAsync(planVersionId, ct);
        if (planWithItems is null || planWithItems.ProposalId != proposalId)
            return;

        if (planWithItems.Status == PlanVersionStatus.Accepted)
        {
            if (run.Status != HiringAgentRunStatus.Hired)
            {
                run.Status = HiringAgentRunStatus.Hired;
                run.CompletedAt ??= DateTime.UtcNow;
                run.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);
            }
            return;
        }

        if (planWithItems.Status != PlanVersionStatus.Proposed)
            return;

        var project = run.Project ?? await _projectRepository.GetByIdAsync(run.ProjectId, ct);
        if (project is null)
            return;

        run.RecommendedPlanVersionId = planWithItems.Id;
        selected.LatestPlanVersionId = planWithItems.Id;

        var recent = selected.ChatRoomId is Guid roomId
            ? (await _messageRepository.GetLatestAsync(roomId, TranscriptMessageLimit, ct))
                .OrderBy(m => m.CreatedAt)
                .ToList()
            : [];

        var review = await _discussionAgent.GenerateFinalPlanReviewAsync(
            BuildProjectContext(project),
            selected.DisplayName,
            BuildTranscript(recent),
            hasMilestonePlan: true,
            planWithItems.Status.ToString(),
            BuildPlanSummary(planWithItems),
            ct);

        if (review.RequestPlanChanges)
        {
            var changeComment = string.IsNullOrWhiteSpace(review.ChangeComment)
                ? review.Message
                : review.ChangeComment!;

            if (string.IsNullOrWhiteSpace(changeComment))
            {
                _logger.LogWarning(
                    "Post-hire plan review for run {RunId} asked for changes without a comment; skipping.",
                    run.Id);
                return;
            }

            var changes = await _milestoneService.RequestPlanChangesAsClientAsync(
                planWithItems.Id,
                run.ClientUserId,
                changeComment,
                isAgentGenerated: true,
                ct);

            if (!changes.IsSuccess)
            {
                _logger.LogWarning(
                    "Post-hire RequestPlanChanges failed for run {RunId}: {Error}",
                    run.Id,
                    changes.Error?.message ?? changes.Message);
                return;
            }

            selected.DiscussionNotes = string.IsNullOrWhiteSpace(selected.DiscussionNotes)
                ? "Post-hire review: requested plan changes"
                : $"{selected.DiscussionNotes}\nPost-hire review: requested plan changes";
            selected.UpdatedAt = DateTime.UtcNow;
            run.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        var accept = await _milestoneService.AcceptPlanAsClientAsync(
            planWithItems.Id,
            run.ClientUserId,
            ct);

        if (!accept.IsSuccess)
        {
            _logger.LogWarning(
                "Post-hire AcceptPlan failed for run {RunId}: {Error}",
                run.Id,
                accept.Error?.message ?? accept.Message);
            return;
        }

        // Reload — AcceptPlan saves independently.
        run = await _runRepository.GetByIdWithCandidatesAsync(run.Id, ct) ?? run;
        run.Status = HiringAgentRunStatus.Hired;
        run.CompletedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        run.RecommendedPlanVersionId = planVersionId;
        await _unitOfWork.SaveChangesAsync(ct);

        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(run.ClientUserId, ct);

        if (clientProfileId is not null)
        {
            BackgroundJob.Enqueue(() =>
                _notificationService.CreateNotification(
                    new CreateNotificationRequest
                    {
                        ClientProfileId = clientProfileId.Value,
                        Title = "Hire complete",
                        Body =
                            $"The hiring agent accepted the revised milestone plan from {selected.DisplayName} and completed the hire for \"{project.Title}\".",
                        Type = NotificationType.ProposalAccepted,
                        ProjectId = project.Id,
                        ProjectProposalId = proposalId,
                        ActionUrl = $"/projects/{project.Id}?tab=milestones"
                    }));
        }
    }

    public async Task<ApiResponse> DismissAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdAsync(runId, ct);
        if (run is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(HiringAgentRun), runId));

        if (run.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("You do not own this hiring agent run."));

        if (run.Status != HiringAgentRunStatus.ReportReady)
            return ApiResponse.Failure(AppError.Validation("Only a ready report can be dismissed."));

        run.Status = HiringAgentRunStatus.Dismissed;
        run.CompletedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Hiring agent report dismissed.");
    }

    public async Task<ApiResponse> CancelAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _runRepository.GetByIdWithCandidatesAsync(runId, ct);
        if (run is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(HiringAgentRun), runId));

        if (run.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("You do not own this hiring agent run."));

        if (!ActiveRunStatuses.Contains(run.Status))
            return ApiResponse.Failure(AppError.Validation("Only an active hiring agent run can be cancelled."));

        var now = DateTime.UtcNow;
        foreach (var candidate in run.Candidates.Where(c => c.Status == HiringAgentCandidateStatus.Invited))
        {
            candidate.Status = HiringAgentCandidateStatus.Expired;
            candidate.UpdatedAt = now;
            if (candidate.InvitationId is Guid invitationId)
            {
                var invitation = await _invitationRepository.GetByIdAsync(invitationId, ct);
                if (invitation is not null && invitation.Status == ProjectInvitationStatus.Pending)
                {
                    invitation.Status = ProjectInvitationStatus.Cancelled;
                    invitation.RespondedAt = now;
                    invitation.RespondedByUserId = _currentUser.UserId;
                    invitation.UpdatedAt = now;
                    invitation.UpdatedBy = _currentUser.UserId.ToString();
                    _invitationRepository.Update(invitation);
                }
            }
        }

        run.Status = HiringAgentRunStatus.Cancelled;
        run.CompletedAt = now;
        run.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Hiring agent run cancelled.");
    }

    public Task<bool> HasActiveRunForProjectAsync(Guid projectId, CancellationToken ct = default)
        => _runRepository.HasActiveRunForProjectAsync(projectId, ct);

    public async Task<int> GetActiveDiscussionLimitAsync(Guid projectId, CancellationToken ct = default)
    {
        var active = await _runRepository.GetActiveByProjectIdAsync(projectId, ct);
        return active?.TopK ?? 1;
    }

    private async Task<Guid?> CreateHiringInviteAsync(
        Guid runId,
        Guid projectId,
        Guid clientUserId,
        string projectTitle,
        HiringAgentCandidate candidate,
        CancellationToken ct)
    {
        try
        {
            if (candidate.InviteeType == ApplicantType.User)
            {
                if (candidate.InviteeUserId is null || candidate.InviteeUserId == Guid.Empty)
                    return null;

                if (candidate.InviteeUserId == clientUserId)
                    return null;

                if (!await _developerProfileRepository.ExistsForUserAsync(candidate.InviteeUserId.Value, ct))
                    return null;

                var existingUserInvite = await FindPendingInviteAsync(
                    projectId, ApplicantType.User, candidate.InviteeUserId.Value, ct);
                if (existingUserInvite is not null)
                    return existingUserInvite.Id;
            }
            else
            {
                if (candidate.InviteeTeamId is null || candidate.InviteeTeamId == Guid.Empty)
                    return null;

                var team = await _teamRepository.GetByIdAsync(candidate.InviteeTeamId.Value, ct);
                if (team is null)
                    return null;

                var existingTeamInvite = await FindPendingInviteAsync(
                    projectId, ApplicantType.Team, candidate.InviteeTeamId.Value, ct);
                if (existingTeamInvite is not null)
                    return existingTeamInvite.Id;
            }

            var invitation = new ProjectInvitation
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ClientUserId = clientUserId,
                InviteeType = candidate.InviteeType,
                InviteeUserId = candidate.InviteeType == ApplicantType.User ? candidate.InviteeUserId : null,
                InviteeTeamId = candidate.InviteeType == ApplicantType.Team ? candidate.InviteeTeamId : null,
                Message =
                    $"You've been invited by FreeGency's AI Hiring Agent to discuss \"{projectTitle}\". " +
                    "Accept to open a discussion and negotiate a milestone plan with the client.",
                Status = ProjectInvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = clientUserId.ToString()
            };

            await _invitationRepository.AddAsync(invitation, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var projectStub = new Project { Id = projectId, Title = projectTitle };
            await NotifyInviteReceivedAsync(invitation, projectStub, ct);
            return invitation.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to create hiring-agent invitation for run {RunId}, candidate {CandidateId}",
                runId,
                candidate.Id);
            _unitOfWork.ClearChangeTracker();
            return null;
        }
    }

    private async Task NotifyInviteReceivedAsync(
        ProjectInvitation invitation,
        Project project,
        CancellationToken ct)
    {
        if (invitation.InviteeType == ApplicantType.User && invitation.InviteeUserId.HasValue)
        {
            var developerProfileId =
                await _userRepository.GetDeveloperProfileIdByUserIdAsync(invitation.InviteeUserId.Value, ct);
            if (developerProfileId is null) return;

            BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
            {
                DeveloperProfileId = developerProfileId,
                Title = "Project invitation",
                Body = $"You've been invited to discuss \"{project.Title}\".",
                Type = NotificationType.InviteReceived,
                ProjectId = project.Id,
                ActionUrl = "/developer/teams?tab=invitations"
            }));
            return;
        }

        if (invitation.InviteeTeamId is null) return;
        var leaders = await _teamMemberRepository.GetLeadersAsync(invitation.InviteeTeamId.Value, ct);
        foreach (var leader in leaders)
        {
            var developerProfileId =
                await _userRepository.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
            if (developerProfileId is null) continue;

            var teamId = invitation.InviteeTeamId.Value;
            BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
            {
                DeveloperProfileId = developerProfileId,
                Title = "Team project invitation",
                Body = $"Your team was invited to discuss \"{project.Title}\".",
                Type = NotificationType.InviteReceived,
                ProjectId = project.Id,
                TeamId = teamId,
                ActionUrl = $"/developer/teams/{teamId}?tab=invitations"
            }));
        }
    }

    private async Task<ProjectInvitation?> FindPendingInviteAsync(
        Guid projectId,
        ApplicantType inviteeType,
        Guid inviteeId,
        CancellationToken ct)
    {
        var pending = await _invitationRepository.GetPendingByProjectIdAsync(projectId, ct);
        return inviteeType switch
        {
            ApplicantType.User => pending.FirstOrDefault(i => i.InviteeUserId == inviteeId),
            ApplicantType.Team => pending.FirstOrDefault(i => i.InviteeTeamId == inviteeId),
            _ => null
        };
    }

    private async Task FailRunAsync(HiringAgentRun run, string reason, CancellationToken ct)
    {
        run.Status = HiringAgentRunStatus.Failed;
        run.FailureReason = reason;
        run.CompletedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var project = run.Project ?? await _projectRepository.GetByIdAsync(run.ProjectId, ct);
        await NotifyClientAsync(
            run.ClientUserId,
            "Hiring agent failed",
            reason,
            NotificationType.HiringAgentFailed,
            run.ProjectId,
            project is null ? "/client/hire-talent" : $"/client/projects/{run.ProjectId}",
            ct);
    }

    private async Task<int> ExpirePendingInvitesAsync(
        HiringAgentRun run,
        DateTime now,
        CancellationToken ct)
    {
        var expired = 0;
        foreach (var invited in run.Candidates.Where(c => c.Status == HiringAgentCandidateStatus.Invited).ToList())
        {
            invited.Status = HiringAgentCandidateStatus.Expired;
            invited.UpdatedAt = now;
            expired++;

            if (invited.InvitationId is not Guid invitationId)
                continue;

            var invitation = await _invitationRepository.GetByIdAsync(invitationId, ct);
            if (invitation is null || invitation.Status != ProjectInvitationStatus.Pending)
                continue;

            invitation.Status = ProjectInvitationStatus.Cancelled;
            invitation.RespondedAt = now;
            invitation.UpdatedAt = now;
            invitation.UpdatedBy = "hiring-agent";
            _invitationRepository.Update(invitation);
        }

        if (expired > 0)
            run.UpdatedAt = now;

        return expired;
    }

    private async Task TryRepairOpenInviteWindowAsync(HiringAgentRun run, CancellationToken ct)
    {
        if (run.Status is not (HiringAgentRunStatus.WaitingAccepts or HiringAgentRunStatus.Discussing))
            return;

        var now = DateTime.UtcNow;
        if (now >= run.InviteDeadlineUtc)
            return;

        var reopened = await ReopenPrematurelyExpiredInvitesAsync(run, now, ct);
        if (reopened > 0)
            await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Attach any InDiscussion proposals that were missed when the run started,
    /// so the report board shows open chats under "Already in discussion".
    /// </summary>
    private async Task EnsureExistingDiscussionsAttachedAsync(HiringAgentRun run, CancellationToken ct)
    {
        // Only after match/invite finished — attaching earlier would short-circuit invite sending.
        if (run.Status is not (HiringAgentRunStatus.WaitingAccepts or HiringAgentRunStatus.Discussing))
            return;

        var openDiscussions = (await _proposalRepository.GetByProjectIdAsync(run.ProjectId, ct: ct))
            .Where(p => p.Status == ProposalStatus.InDiscussion)
            .ToList();
        if (openDiscussions.Count == 0)
            return;

        var existingKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in run.Candidates)
        {
            var key = ApplicantKey(
                c.InviteeType,
                c.InviteeType == ApplicantType.Team ? c.InviteeTeamId : c.InviteeUserId);
            if (key is not null)
                existingKeys.Add(key);

            if (c.ProposalId is Guid proposalId)
                existingKeys.Add($"proposal:{proposalId:N}");
        }

        var toAdd = new List<HiringAgentCandidate>();
        var kickoffIds = new List<Guid>();
        var rank = run.Candidates.Count == 0 ? 0 : run.Candidates.Max(c => c.RankOrder);

        foreach (var proposal in openDiscussions)
        {
            var key = ApplicantKey(
                proposal.ApplicantType,
                proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : proposal.UserId);
            if (key is null)
                continue;
            if (existingKeys.Contains(key) || existingKeys.Contains($"proposal:{proposal.Id:N}"))
                continue;

            existingKeys.Add(key);
            rank++;
            var room = await _chatRoomRepository.GetByProposalIdAsync(proposal.Id, ct);
            var (name, avatar) = await ResolveApplicantDisplayAsync(proposal, ct);
            var candidate = new HiringAgentCandidate
            {
                Id = Guid.NewGuid(),
                HiringAgentRunId = run.Id,
                InviteeType = proposal.ApplicantType,
                InviteeUserId = proposal.ApplicantType == ApplicantType.User ? proposal.UserId : null,
                InviteeTeamId = proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : null,
                DisplayName = name,
                AvatarUrl = avatar,
                SuggestionScore = 0f,
                RankOrder = rank,
                Status = HiringAgentCandidateStatus.Discussing,
                ProposalId = proposal.Id,
                ChatRoomId = room?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = run.ClientUserId.ToString()
            };
            toAdd.Add(candidate);
            if (room is not null)
                kickoffIds.Add(candidate.Id);
        }

        if (toAdd.Count == 0)
            return;

        _unitOfWork.ClearChangeTracker();
        await _runRepository.PersistMatchInviteResultsAsync(run.Id, toAdd, ct);

        // Reload so callers map the updated candidate list.
        var refreshed = await _runRepository.GetByIdWithCandidatesAsync(run.Id, ct);
        if (refreshed is not null)
        {
            run.Candidates = refreshed.Candidates;
            run.Status = refreshed.Status;
            run.UpdatedAt = refreshed.UpdatedAt;
        }

        foreach (var candidateId in kickoffIds)
        {
            BackgroundJob.Enqueue<IHiringAgentService>(s =>
                s.ProcessDiscussionTurnAsync(candidateId, CancellationToken.None));
        }
    }

    /// <summary>
    /// Undo early auto-expire while the invite window is still open so candidates
    /// show as Pending again (and can still accept).
    /// </summary>
    private async Task<int> ReopenPrematurelyExpiredInvitesAsync(
        HiringAgentRun run,
        DateTime now,
        CancellationToken ct)
    {
        var reopened = 0;
        foreach (var candidate in run.Candidates
                     .Where(c => c.Status == HiringAgentCandidateStatus.Expired)
                     .ToList())
        {
            if (candidate.InvitationId is not Guid invitationId)
                continue;

            var invitation = await _invitationRepository.GetByIdAsync(invitationId, ct);
            if (invitation is null)
                continue;

            // Only reopen system-cancelled invites that nobody answered.
            if (invitation.Status != ProjectInvitationStatus.Cancelled)
                continue;
            if (!string.Equals(invitation.UpdatedBy, "hiring-agent", StringComparison.Ordinal))
                continue;
            if (invitation.RespondedByUserId is not null)
                continue;

            invitation.Status = ProjectInvitationStatus.Pending;
            invitation.RespondedAt = null;
            invitation.RespondedByUserId = null;
            invitation.UpdatedAt = now;
            invitation.UpdatedBy = "hiring-agent-reopen";
            _invitationRepository.Update(invitation);

            candidate.Status = HiringAgentCandidateStatus.Invited;
            candidate.UpdatedAt = now;
            reopened++;
        }

        if (reopened > 0)
            run.UpdatedAt = now;

        return reopened;
    }

    private static int Clamp(int value, int min, int max)
        => Math.Min(max, Math.Max(min, value));

    /// <summary>
    /// Safety net: discussion-phase replies must not ask for plan revisions in chat.
    /// </summary>
    private static bool LooksLikePlanRevisionAsk(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return false;
        var text = message.ToLowerInvariant();
        return text.Contains("revise", StringComparison.Ordinal)
            || text.Contains("revision", StringComparison.Ordinal)
            || text.Contains("update the plan", StringComparison.Ordinal)
            || text.Contains("update your plan", StringComparison.Ordinal)
            || text.Contains("change the plan", StringComparison.Ordinal)
            || text.Contains("change your plan", StringComparison.Ordinal)
            || text.Contains("adjust the due", StringComparison.Ordinal)
            || text.Contains("adjust those date", StringComparison.Ordinal)
            || text.Contains("more immediate schedule", StringComparison.Ordinal)
            || text.Contains("request changes", StringComparison.Ordinal);
    }

    private async Task NotifyClientAsync(
        Guid clientUserId,
        string title,
        string body,
        NotificationType type,
        Guid projectId,
        string actionUrl,
        CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(clientUserId, ct);
        if (clientProfileId is null) return;

        BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = title,
            Body = body,
            Type = type,
            ProjectId = projectId,
            ActionUrl = actionUrl
        }));
    }

    private static string BuildProjectContext(Project project)
    {
        return $"""
            Title: {project.Title}
            Description: {project.Description}
            Budget: {project.BudgetMin:0.##} - {project.BudgetMax:0.##} {project.Currency}
            Deadline: {(project.Deadline?.ToString("yyyy-MM-dd") ?? "n/a")}
            Estimated days: {project.EstimatedDurationDays?.ToString() ?? "n/a"}
            Fixed price: {project.IsFixedPrice}
            """;
    }

    private static string BuildPlanSummary(MilestonePlanVersion? plan)
    {
        if (plan is null)
            return "(none)";

        var items = plan.Items?
            .OrderBy(i => i.SortOrder)
            .Select(i =>
                $"- {i.Title} | {i.Amount:0.##} | due {(i.DueDate?.ToString("yyyy-MM-dd") ?? "n/a")} | DoD: {i.DefinitionOfDone}")
            .ToList() ?? [];

        var body = items.Count == 0 ? "(no milestone items)" : string.Join('\n', items);
        return $"""
            Version: {plan.Version}
            Status: {plan.Status}
            ChangeComment: {plan.ChangeComment ?? "(none)"}
            Items:
            {body}
            """;
    }

    private static string BuildTranscript(IEnumerable<Message> messages)
    {
        var lines = messages.Select(m =>
        {
            var who = m.MessageType == MessageType.System
                ? "System"
                : m.SenderClientProfileId is not null
                    ? (m.IsAgentGenerated ? "Client(AI)" : "Client")
                    : m.SenderDeveloperProfileId is not null
                        ? "Freelancer"
                        : "Unknown";
            var text = m.Text ?? (m.FileName is null ? "[attachment]" : $"[file: {m.FileName}]");
            return $"[{m.CreatedAt:u}] {who}: {text}";
        });
        return string.Join('\n', lines);
    }

    private static HiringAgentRunDto MapRunDto(HiringAgentRun run) => new()
    {
        Id = run.Id,
        ProjectId = run.ProjectId,
        ProjectTitle = run.Project?.Title ?? string.Empty,
        Status = run.Status,
        TopK = run.TopK,
        InviteDeadlineUtc = run.InviteDeadlineUtc,
        DiscussionDeadlineUtc = run.DiscussionDeadlineUtc,
        RecommendedProposalId = run.RecommendedProposalId,
        RecommendedPlanVersionId = run.RecommendedPlanVersionId,
        RecommendedCandidateId = run.RecommendedCandidateId,
        FailureReason = run.FailureReason,
        CreatedAt = run.CreatedAt,
        ReportReadyAt = run.ReportReadyAt,
        CompletedAt = run.CompletedAt,
        ClientHireApprovedAt = run.ClientHireApprovedAt,
        Candidates = run.Candidates
            .OrderBy(c => c.RankOrder)
            .Select(MapCandidateDto)
            .ToList()
    };

    private static HiringAgentCandidateDto MapCandidateDto(HiringAgentCandidate c)
    {
        var alreadyApplied = c.ProposalId is not null && c.InvitationId is null;
        var aiRecommended = c.SuggestionScore > 0 || c.InvitationId is not null;
        // Prefer status over score so open discussions aren't labeled as scout-invite/applied.
        string sourceGroup;
        if (c.InvitationId is not null)
            sourceGroup = "scout-invite";
        else if (c.ProposalId is not null &&
                 c.Status is HiringAgentCandidateStatus.Discussing
                     or HiringAgentCandidateStatus.PlanProposed
                     or HiringAgentCandidateStatus.Ranked)
            sourceGroup = "existing-discussion";
        else if (alreadyApplied)
            sourceGroup = aiRecommended ? "applied-and-recommended" : "existing-discussion";
        else
            sourceGroup = "scout-invite";

        return new HiringAgentCandidateDto
        {
            Id = c.Id,
            InviteeType = c.InviteeType,
            InviteeUserId = c.InviteeUserId,
            InviteeTeamId = c.InviteeTeamId,
            DisplayName = c.DisplayName,
            AvatarUrl = c.AvatarUrl,
            SuggestionScore = c.SuggestionScore,
            RankOrder = c.RankOrder,
            Status = c.Status,
            InvitationId = c.InvitationId,
            ProposalId = c.ProposalId,
            ChatRoomId = c.ChatRoomId,
            LatestPlanVersionId = c.LatestPlanVersionId,
            DiscussionScore = c.DiscussionScore,
            DiscussionNotes = c.DiscussionNotes,
            AgentMessageCount = c.AgentMessageCount,
            AlreadyApplied = alreadyApplied,
            AiRecommended = aiRecommended,
            SourceGroup = sourceGroup
        };
    }
}
