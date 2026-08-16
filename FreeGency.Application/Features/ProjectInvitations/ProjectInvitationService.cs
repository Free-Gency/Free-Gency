using FreeGency.Application.Features.ProjectInvitations.Dtos;


namespace FreeGency.Application.Features.ProjectInvitations;

public class ProjectInvitationService : IProjectInvitationService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeveloperProfileRepository _developerProfileRepository;
    private readonly IEntitlementService _entitlementService;

    public ProjectInvitationService(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IEntitlementService entitlementService)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _entitlementService = entitlementService;
        _invitationRepository = unitOfWork.Repository<IProjectInvitationRepository, ProjectInvitation>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _teamRepository = unitOfWork.Repository<ITeamRepository, Team>();
        _teamMemberRepository = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _chatRoomRepository = unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        _messageRepository = unitOfWork.Repository<IMessageRepository, Message>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
    }

    public async Task<ApiResponse<ProjectInvitationDto>> CreateAsync(
        CreateProjectInvitationDto dto,
        CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<ProjectInvitationDto>(AppError.NotFound(nameof(Project), dto.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<ProjectInvitationDto>(
                AppError.Forbidden("Only the project's client can send invitations."));

        if (project.Status != ProjectStatus.Open)
            return ApiResponse.Failure<ProjectInvitationDto>(
                AppError.Validation("Project is not open for invitations."));

        var hiringAgent = _unitOfWork.Repository<IHiringAgentRunRepository, HiringAgentRun>();
        var activeAgentRun = await hiringAgent.GetActiveByProjectIdAsync(dto.ProjectId, ct);
        var discussionLimit = activeAgentRun?.TopK ?? 1;

        var activeDiscussions =
            (await _proposalRepository.GetActiveDiscussionByProjectIdAsync(dto.ProjectId, ct)).ToList();
        if (activeDiscussions.Count >= discussionLimit)
            return ApiResponse.Failure<ProjectInvitationDto>(
                AppError.Validation(
                    discussionLimit <= 1
                        ? "This project already has an active discussion. Close it before sending invitations."
                        : $"This project already has {activeDiscussions.Count} active discussions (limit {discussionLimit})."));


        // check entitlement:
        var quota = await _entitlementService.CanConsumeAsync(_currentUser.UserId, FeatureType.ProjectSendInvitation, ct);
        if (!quota.IsAllowed)
            return ApiResponse.Failure<ProjectInvitationDto>(quota.ToAppError());

        if (dto.InviteeType == ApplicantType.User)
        {
            if (dto.InviteeUserId is null || dto.InviteeUserId == Guid.Empty)
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.Validation("InviteeUserId is required."));

            if (dto.InviteeUserId == _currentUser.UserId)
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.Validation("You cannot invite yourself."));

            var exists = await _developerProfileRepository.ExistsForUserAsync(dto.InviteeUserId.Value, ct);
            if (!exists)
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.NotFound("Developer", dto.InviteeUserId.Value));

            if (await _invitationRepository.HasPendingAsync(dto.ProjectId, ApplicantType.User, dto.InviteeUserId.Value, ct))
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.Validation("A pending invitation already exists for this developer."));
        }
        else
        {
            if (dto.InviteeTeamId is null || dto.InviteeTeamId == Guid.Empty)
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.Validation("InviteeTeamId is required."));

            var team = await _teamRepository.GetByIdAsync(dto.InviteeTeamId.Value, ct);
            if (team is null)
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.NotFound(nameof(Team), dto.InviteeTeamId.Value));

            if (await _invitationRepository.HasPendingAsync(dto.ProjectId, ApplicantType.Team, dto.InviteeTeamId.Value, ct))
                return ApiResponse.Failure<ProjectInvitationDto>(
                    AppError.Validation("A pending invitation already exists for this team."));
        }

        var invitation = new ProjectInvitation
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            ClientUserId = _currentUser.UserId,
            InviteeType = dto.InviteeType,
            InviteeUserId = dto.InviteeType == ApplicantType.User ? dto.InviteeUserId : null,
            InviteeTeamId = dto.InviteeType == ApplicantType.Team ? dto.InviteeTeamId : null,
            Message = dto.Message.Trim(),
            Status = ProjectInvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId.ToString()
        };

        await _invitationRepository.AddAsync(invitation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await NotifyInviteReceivedAsync(invitation, project, ct);

        var loaded = await _invitationRepository.GetByIdWithDetailsAsync(invitation.Id, ct)
                     ?? invitation;
        return ApiResponse.Success(MapDto(loaded), "Invitation sent.");
    }

    public async Task<ApiResponse<IReadOnlyList<ProjectInvitationDto>>> GetSentAsync(
        FilterProjectInvitationsDto filter,
        CancellationToken ct = default)
    {
        var items = await _invitationRepository.GetSentByClientAsync(
            _currentUser.UserId,
            filter.Status,
            ct);
        return ApiResponse.Success<IReadOnlyList<ProjectInvitationDto>>(items.Select(MapDto).ToList());
    }

    public async Task<ApiResponse<IReadOnlyList<ProjectInvitationDto>>> GetReceivedAsync(
        FilterProjectInvitationsDto filter,
        CancellationToken ct = default)
    {
        var memberships = await _teamMemberRepository.GetByUserIdAsync(_currentUser.UserId, ct);
        var leaderTeamIds = memberships
            .Where(m => m.TeamRole == Role.TeamLeader)
            .Select(m => m.TeamId)
            .Distinct()
            .ToList();

        // Also include owned teams even if role row is missing.
        var owned = await _teamRepository.Query()
            .Where(t => t.OwnerUserId == _currentUser.UserId && !t.IsDeleted)
            .Select(t => t.Id)
            .ToListAsync(ct);
        foreach (var id in owned)
        {
            if (!leaderTeamIds.Contains(id))
                leaderTeamIds.Add(id);
        }

        var items = await _invitationRepository.GetReceivedForDeveloperAsync(
            _currentUser.UserId,
            leaderTeamIds,
            filter.Status,
            ct);
        return ApiResponse.Success<IReadOnlyList<ProjectInvitationDto>>(items.Select(MapDto).ToList());
    }

    public async Task<ApiResponse<IReadOnlyList<ProjectInvitationDto>>> GetForTeamAsync(
        Guid teamId,
        FilterProjectInvitationsDto filter,
        CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<IReadOnlyList<ProjectInvitationDto>>(
                AppError.NotFound(nameof(Team), teamId));

        var isLeader = await _teamMemberRepository.IsLeaderAsync(teamId, _currentUser.UserId, ct)
                       || team.OwnerUserId == _currentUser.UserId;
        if (!isLeader)
            return ApiResponse.Failure<IReadOnlyList<ProjectInvitationDto>>(
                AppError.Forbidden("Only team leaders can view team invitations."));

        var items = await _invitationRepository.GetForTeamAsync(teamId, filter.Status, ct);
        return ApiResponse.Success<IReadOnlyList<ProjectInvitationDto>>(items.Select(MapDto).ToList());
    }

    public async Task<ApiResponse<Guid>> AcceptAsync(Guid invitationId, CancellationToken ct = default)
    {
        var invitation = await _invitationRepository.GetByIdWithDetailsAsync(invitationId, ct);
        if (invitation is null)
            return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(ProjectInvitation), invitationId));

        if (invitation.Status != ProjectInvitationStatus.Pending)
            return ApiResponse.Failure<Guid>(AppError.Validation("Invitation is no longer pending."));

        var auth = await EnsureCanRespondAsync(invitation, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure<Guid>(auth.Error!);

        var project = invitation.Project
                      ?? await _projectRepository.GetByIdAsync(invitation.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(Project), invitation.ProjectId));

        if (project.Status != ProjectStatus.Open)
            return ApiResponse.Failure<Guid>(AppError.Validation("Project is not open."));

        var applicantType = invitation.InviteeType;
        var applicantId = applicantType == ApplicantType.Team
            ? invitation.InviteeTeamId!.Value
            : invitation.InviteeUserId!.Value;

        // Reuse an existing proposal/discussion for the same invitee when possible.
        var existingForInvitee = await FindOpenProposalForApplicantAsync(
            invitation.ProjectId, applicantType, applicantId, ct);

        if (existingForInvitee is not null && existingForInvitee.Status == ProposalStatus.InDiscussion)
        {
            var existingRoom = await _chatRoomRepository.GetByProposalIdAsync(existingForInvitee.Id, ct);
            if (existingRoom is null)
                return ApiResponse.Failure<Guid>(
                    AppError.Validation("Discussion chat room was not found for the existing proposal."));

            await MarkInvitationAcceptedAsync(invitation, existingForInvitee.Id, existingRoom.Id, ct);
            await NotifyInviteAcceptedAsync(invitation, project, existingRoom.Id, ct);
            BackgroundJob.Enqueue<IHiringAgentService>(s =>
                s.OnInvitationAcceptedAsync(invitationId, existingForInvitee.Id, existingRoom.Id, CancellationToken.None));
            return ApiResponse.Success(existingRoom.Id, "Invitation accepted. Discussion already open.");
        }

        // Allow a second concurrent discussion (e.g. client invited A, then opened discussion with B).
        ProjectProposal proposal;
        if (existingForInvitee is not null)
        {
            proposal = existingForInvitee;
            await _proposalRepository.UpdateStatusAsync(proposal.Id, ProposalStatus.InDiscussion, ct);
            proposal = await _proposalRepository.GetByIdAsync(proposal.Id, ct)
                       ?? proposal;
        }
        else
        {
            proposal = new ProjectProposal
            {
                Id = Guid.NewGuid(),
                ProjectId = invitation.ProjectId,
                ApplicantType = applicantType,
                TeamId = applicantType == ApplicantType.Team ? invitation.InviteeTeamId : null,
                UserId = _currentUser.UserId,
                CoverLetter = invitation.Message,
                Approach = "Accepted project invitation — ready to discuss scope and milestones.",
                ProposedTimeline = null,
                SimilarLinksUrl = null,
                ProposedBudget = project.BudgetMin > 0 ? project.BudgetMin : 1m,
                Status = ProposalStatus.InDiscussion,
                AppliedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId.ToString()
            };

            await _proposalRepository.AddAsync(proposal, ct);
        }

        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(project.ClientId, ct);
        if (clientProfileId is null)
            return ApiResponse.Failure<Guid>(AppError.Validation("Client profile is required to open discussion."));

        var members = new List<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>
        {
            (clientProfileId, null, true, "Client")
        };

        if (applicantType == ApplicantType.User)
        {
            var developerProfileId =
                await _userRepository.GetDeveloperProfileIdByUserIdAsync(invitation.InviteeUserId!.Value, ct);
            if (developerProfileId is null)
                return ApiResponse.Failure<Guid>(AppError.Validation("Developer profile was not found."));
            members.Add((null, developerProfileId, true, null));
        }
        else
        {
            var leaders = await _teamMemberRepository.GetLeadersAsync(invitation.InviteeTeamId!.Value, ct);
            var added = new HashSet<Guid>();
            foreach (var leader in leaders)
            {
                var developerProfileId =
                    await _userRepository.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
                if (developerProfileId is null)
                    continue;

                var canSend = leader.UserId == _currentUser.UserId;
                members.Add((null, developerProfileId, canSend, canSend ? "Team Leader" : "Team Leader (view only)"));
                added.Add(developerProfileId.Value);
            }

            var speakerProfileId =
                await _userRepository.GetDeveloperProfileIdByUserIdAsync(_currentUser.UserId, ct);
            if (speakerProfileId is not null && added.Add(speakerProfileId.Value))
                members.Add((null, speakerProfileId, true, "Team Leader"));

            if (members.Count < 2)
                return ApiResponse.Failure<Guid>(
                    AppError.Validation("Team leader developer profile was not found."));
        }

        var chatRoom = await _chatRoomRepository.GetByProposalIdForUpdateAsync(proposal.Id, ct);
        if (chatRoom is null)
        {
            chatRoom = new ChatRoom
            {
                Id = Guid.NewGuid(),
                RoomType = RoomType.Proposal,
                Status = ChatRoomStatus.Active,
                ProposalId = proposal.Id,
                TeamId = proposal.TeamId,
                ProjectId = null,
                Title = project.Title,
                CreatedByUserId = project.ClientId,
                CreatedAt = DateTime.UtcNow
            };

            await _chatRoomRepository.AddWithMembersAsync(chatRoom, members, ct);
            await _messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoom.Id,
                MessageType = MessageType.System,
                Text = "Invitation accepted. Discussion started — negotiate the Milestone Plan next."
            }, ct);
        }
        else if (chatRoom.Status == ChatRoomStatus.Archived)
        {
            chatRoom.Status = ChatRoomStatus.Active;
            chatRoom.ArchivedAt = null;
            _chatRoomRepository.Update(chatRoom);
            await _messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoom.Id,
                MessageType = MessageType.System,
                Text = "Invitation accepted. Discussion reopened."
            }, ct);
        }

        await MarkInvitationAcceptedAsync(invitation, proposal.Id, chatRoom.Id, ct);
        await NotifyInviteAcceptedAsync(invitation, project, chatRoom.Id, ct);
        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.OnInvitationAcceptedAsync(invitationId, proposal.Id, chatRoom.Id, CancellationToken.None));

        return ApiResponse.Success(chatRoom.Id, "Invitation accepted. Discussion opened.");
    }

    private async Task<ProjectProposal?> FindOpenProposalForApplicantAsync(
        Guid projectId,
        ApplicantType applicantType,
        Guid applicantId,
        CancellationToken ct)
    {
        var open = await _proposalRepository.GetByProjectIdAsync(projectId, status: null, ct);
        return open.FirstOrDefault(p =>
            p.Status is ProposalStatus.Pending or ProposalStatus.Viewed or ProposalStatus.InDiscussion
            && (applicantType == ApplicantType.Team
                ? p.TeamId == applicantId
                : p.UserId == applicantId && p.ApplicantType == ApplicantType.User));
    }

    private async Task MarkInvitationAcceptedAsync(
        ProjectInvitation invitation,
        Guid proposalId,
        Guid chatRoomId,
        CancellationToken ct)
    {
        invitation.Status = ProjectInvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;
        invitation.RespondedByUserId = _currentUser.UserId;
        invitation.ProposalId = proposalId;
        invitation.ChatRoomId = chatRoomId;
        invitation.UpdatedAt = DateTime.UtcNow;
        invitation.UpdatedBy = _currentUser.UserId.ToString();
        _invitationRepository.Update(invitation);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<ApiResponse> RejectAsync(Guid invitationId, CancellationToken ct = default)
    {
        var invitation = await _invitationRepository.GetByIdWithDetailsAsync(invitationId, ct);
        if (invitation is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectInvitation), invitationId));

        if (invitation.Status != ProjectInvitationStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Invitation is no longer pending."));

        var auth = await EnsureCanRespondAsync(invitation, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        invitation.Status = ProjectInvitationStatus.Rejected;
        invitation.RespondedAt = DateTime.UtcNow;
        invitation.RespondedByUserId = _currentUser.UserId;
        invitation.UpdatedAt = DateTime.UtcNow;
        invitation.UpdatedBy = _currentUser.UserId.ToString();
        _invitationRepository.Update(invitation);
        await _unitOfWork.SaveChangesAsync(ct);

        var project = invitation.Project
                      ?? await _projectRepository.GetByIdAsync(invitation.ProjectId, ct);
        if (project is not null)
            await NotifyInviteRejectedAsync(invitation, project, ct);

        BackgroundJob.Enqueue<IHiringAgentService>(s =>
            s.OnInvitationRejectedAsync(invitationId, CancellationToken.None));

        return ApiResponse.Success("Invitation rejected.");
    }

    public async Task<ApiResponse> CancelAsync(Guid invitationId, CancellationToken ct = default)
    {
        var invitation = await _invitationRepository.GetByIdAsync(invitationId, ct);
        if (invitation is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectInvitation), invitationId));

        if (invitation.ClientUserId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the sender can cancel this invitation."));

        if (invitation.Status != ProjectInvitationStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Only pending invitations can be cancelled."));

        invitation.Status = ProjectInvitationStatus.Cancelled;
        invitation.RespondedAt = DateTime.UtcNow;
        invitation.RespondedByUserId = _currentUser.UserId;
        invitation.UpdatedAt = DateTime.UtcNow;
        invitation.UpdatedBy = _currentUser.UserId.ToString();
        _invitationRepository.Update(invitation);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Invitation cancelled.");
    }

    private async Task<ApiResponse> EnsureCanRespondAsync(ProjectInvitation invitation, CancellationToken ct)
    {
        if (invitation.InviteeType == ApplicantType.User)
        {
            if (invitation.InviteeUserId != _currentUser.UserId)
                return ApiResponse.Failure(AppError.Forbidden("Only the invited developer can respond."));
            return ApiResponse.Success();
        }

        var teamId = invitation.InviteeTeamId!.Value;
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Team), teamId));

        var isLeader = await _teamMemberRepository.IsLeaderAsync(teamId, _currentUser.UserId, ct)
                       || team.OwnerUserId == _currentUser.UserId;
        if (!isLeader)
            return ApiResponse.Failure(AppError.Forbidden("Only team leaders can respond to team invitations."));

        return ApiResponse.Success();
    }

    private static ProjectInvitationDto MapDto(ProjectInvitation i)
    {
        static string Name(User? u) =>
            u is null ? string.Empty : $"{u.FristName} {u.LastName}".Trim();

        return new ProjectInvitationDto
        {
            Id = i.Id,
            ProjectId = i.ProjectId,
            ProjectTitle = i.Project?.Title ?? string.Empty,
            ClientUserId = i.ClientUserId,
            ClientName = Name(i.ClientUser),
            InviteeType = i.InviteeType,
            InviteeUserId = i.InviteeUserId,
            InviteeUserName = Name(i.InviteeUser),
            InviteeTeamId = i.InviteeTeamId,
            InviteeTeamName = i.InviteeTeam?.Name,
            Message = i.Message,
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            RespondedAt = i.RespondedAt,
            ProposalId = i.ProposalId,
            ChatRoomId = i.ChatRoomId
        };
    }

    private async Task NotifyInviteReceivedAsync(ProjectInvitation invitation, Project project, CancellationToken ct)
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

    private async Task NotifyInviteAcceptedAsync(
        ProjectInvitation invitation,
        Project project,
        Guid chatRoomId,
        CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(project.ClientId, ct);
        if (clientProfileId is null) return;

        BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Invitation accepted",
            Body = $"Your invitation for \"{project.Title}\" was accepted. Discussion is open.",
            Type = NotificationType.InviteAccepted,
            ProjectId = project.Id,
            ProjectProposalId = invitation.ProposalId,
            ChatRoomId = chatRoomId,
            ActionUrl = $"/client/messages?room={chatRoomId}"
        }));
    }

    private async Task NotifyInviteRejectedAsync(
        ProjectInvitation invitation,
        Project project,
        CancellationToken ct)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(project.ClientId, ct);
        if (clientProfileId is null) return;

        BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Invitation declined",
            Body = $"Your invitation for \"{project.Title}\" was declined.",
            Type = NotificationType.InviteRejected,
            ProjectId = project.Id,
            ActionUrl = "/client/hire-talent?tab=invitations"
        }));
    }
}
