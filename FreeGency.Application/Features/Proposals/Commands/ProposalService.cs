using AutoMapper.Execution;
using FreeGency.Application.Features.NotificationFeature.Commands;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.Proposals.Dtos;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using FreeGency.Infrastructure.Interfaces;
using FreeGency.Infrastructure.Persistence;
using FreeGency.Infrastructure.Persistence.Repositories.Teams;
using Hangfire;

namespace FreeGency.Application.Features.Proposals.Commands;

public partial class ProposalService : IProposalService
{
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;
    private readonly INotificationService _notificationService;
    private readonly ITeamRepository _teamRepository;
    public ProposalService(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStorageService storageService,INotificationService notificationService,ITeamRepository teamRepository)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storageService = storageService;
        _notificationService = notificationService;
        _teamRepository = teamRepository;
        _proposalRepository = _unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _projectRepository = _unitOfWork.Repository<IProjectRepository, Project>();
        _teamMemberRepository = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _chatRoomRepository = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        _messageRepository = _unitOfWork.Repository<IMessageRepository, Message>();
        _userRepository = _unitOfWork.Repository<IUserRepository, User>();
    }

    public async Task<ApiResponse> CreateAsync(CreateProposalDto dto, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), dto.ProjectId));

        if (project.Status != ProjectStatus.Open)
            return ApiResponse.Failure(AppError.Validation("Project is not open for proposals."));

        var applicantId = dto.ApplicantType == ApplicantType.Team
            ? dto.TeamId ?? Guid.Empty
            : _currentUser.UserId;

        if (dto.ApplicantType == ApplicantType.Team && dto.TeamId is null)
            return ApiResponse.Failure(AppError.Validation("TeamId is required when applying as a team."));

        if (dto.ApplicantType == ApplicantType.Team &&
            !await _teamMemberRepository.IsLeaderAsync(dto.TeamId!.Value, _currentUser.UserId, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only a team leader can submit a proposal for the team."));

        if (await _proposalRepository.HasPendingOrActiveAsync(dto.ProjectId, dto.ApplicantType, applicantId, ct))
            return ApiResponse.Failure(AppError.Validation("You already have a pending or active proposal for this project."));

        var proposal = new ProjectProposal
        {
            ProjectId = dto.ProjectId,
            ApplicantType = dto.ApplicantType,
            TeamId = dto.ApplicantType == ApplicantType.Team ? dto.TeamId : null,
            UserId = _currentUser.UserId,
            CoverLetter = dto.CoverLetter,
            Approach = dto.Approach ?? string.Empty,
            ProposedTimeline = dto.ProposedTimeline,
            SimilarLinksUrl = dto.SimilarLinksUrl,
            ProposedBudget = dto.ProposedBudget,
            Status = ProposalStatus.Pending
        };

        var attachments = new List<ProposalAttachment>();
        if (dto.Attachments is { Length: > 0 })
        {
            foreach (var file in dto.Attachments)
            {
                UploadedAsset uploaded;
                try
                {
                    uploaded = await _storageService.UploadAsync(file, StorageFolders.ProposalAttachments, ct);
                }
                catch (Exception)
                {
                    return ApiResponse.Failure(AppError.FileUploadFailed(file.FileName));
                }

                attachments.Add(new ProposalAttachment
                {
                    FileName = string.IsNullOrWhiteSpace(uploaded.FileName) ? file.FileName : uploaded.FileName,
                    FileUrl = uploaded.Url
                });
            }
        }

        await _proposalRepository.AddWithAttachmentsAsync(proposal, attachments, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var clientProfileId =
                   await _userRepository.GetClientProfileIdByUserIdAsync(project.ClientId, ct);

        if (clientProfileId.HasValue)
        {
            string applicantName;

            if (dto.ApplicantType == ApplicantType.Team)
            {
                var team = await _teamRepository.GetByIdAsync(dto.TeamId!.Value, ct);
                if (team is null)
                    return ApiResponse.Failure(AppError.NotFound(nameof(Team), dto.TeamId.Value));
                applicantName = team.Name;
            }
            else
            {
                applicantName = $"{_currentUser.FirstName} {_currentUser.LastName}";
            }
            BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
            {
                ClientProfileId = clientProfileId,
                Title = "New proposal",
                Body = $"{applicantName} submitted a proposal for your project.",
                Type = NotificationType.NewProposal,
                TeamId = dto.TeamId,
                ProjectId = project.Id,
                ProjectProposalId = proposal.Id,

                ActionUrl = $"/projects/{project.Id}?tab=proposals"
            }));
            

        }
        return ApiResponse.Success("Proposal submitted successfully.");
    }

    public async Task<ApiResponse> UpdateAsync(UpdateProposalDto dto, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(dto.Id, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), dto.Id));

        var isOwner = proposal.ApplicantType == ApplicantType.User
            ? proposal.UserId == _currentUser.UserId
            : proposal.TeamId is not null &&
                await _teamMemberRepository.IsLeaderAsync(proposal.TeamId.Value, _currentUser.UserId, ct);

        if (!isOwner)
            return ApiResponse.Failure(AppError.Forbidden("You are not allowed to edit this proposal."));

        if (proposal.Status is not (ProposalStatus.Pending or ProposalStatus.Viewed))
            return ApiResponse.Failure(AppError.Validation("Only pending or viewed proposals can be edited."));

        if (dto.CoverLetter is not null)
            proposal.CoverLetter = dto.CoverLetter;
        if (dto.Approach is not null)
            proposal.Approach = dto.Approach;
        if (dto.ProposedTimeline is not null)
            proposal.ProposedTimeline = dto.ProposedTimeline;
        if (dto.SimilarLinksUrl is not null)
            proposal.SimilarLinksUrl = dto.SimilarLinksUrl;
        if (dto.ProposedBudget.HasValue)
            proposal.ProposedBudget = dto.ProposedBudget.Value;

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Proposal updated successfully.");
    }

    public async Task<ApiResponse> ViewAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var project = await _projectRepository.GetByIdAsync(proposal.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), proposal.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the project's client can view a proposal."));

        if (proposal.Status == ProposalStatus.Pending)
        {
            await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.Viewed, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return ApiResponse.Success("Proposal viewed.");
    }

    /// <summary>
    /// Opens discussion only — does NOT hire and does NOT cascade-reject other proposals.
    /// Returns the proposal chat room id for client navigation.
    /// </summary>
    public async Task<ApiResponse<Guid>> StartDiscussionAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var project = await _projectRepository.GetByIdAsync(proposal.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(Project), proposal.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<Guid>(AppError.Forbidden("Only the project's client can start a discussion."));

        if (project.Status != ProjectStatus.Open)
            return ApiResponse.Failure<Guid>(AppError.Validation("Project is not open for new discussions."));

        if (proposal.Status is ProposalStatus.Rejected or ProposalStatus.Withdrawn or ProposalStatus.Expired or ProposalStatus.Accepted)
            return ApiResponse.Failure<Guid>(AppError.Validation("Cannot discuss a closed proposal."));

        var existingRoom = await _chatRoomRepository.GetByProposalIdForUpdateAsync(proposalId, ct);

        if (proposal.Status == ProposalStatus.InDiscussion)
        {
            if (existingRoom is null)
                return ApiResponse.Failure<Guid>(AppError.Validation("Discussion chat room was not found."));

            return ApiResponse.Success(
                existingRoom.Id,
                "Discussion is already active for this proposal.");
        }

        // Multiple concurrent discussions are allowed (e.g. pending invites + another applicant).
        await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.InDiscussion, ct);

        ChatRoom chatRoom;
        List<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>? notifyMembers = null;

        if (existingRoom is not null)
        {
            chatRoom = existingRoom;
            if (chatRoom.Status == ChatRoomStatus.Archived)
            {
                chatRoom.Status = ChatRoomStatus.Active;
                chatRoom.ArchivedAt = null;
                _chatRoomRepository.Update(chatRoom);
            }
        }
        else
        {
            var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(project.ClientId, ct);
            if (clientProfileId is null)
                return ApiResponse.Failure<Guid>(AppError.Validation("Client profile is required to start a discussion."));

            var members = new List<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>
            {
                (clientProfileId, null, true, "Client")
            };

            if (proposal.ApplicantType == ApplicantType.User && proposal.UserId.HasValue)
            {
                var developerProfileId =
                    await _userRepository.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
                if (developerProfileId is null)
                    return ApiResponse.Failure<Guid>(AppError.Validation("Applicant developer profile was not found."));

                members.Add((null, developerProfileId, true, null));
            }
            else if (proposal.ApplicantType == ApplicantType.Team && proposal.TeamId.HasValue)
            {
                var speakerId = proposal.UserId;
                var leaders = await _teamMemberRepository.GetLeadersAsync(proposal.TeamId.Value, ct);
                var addedDeveloperProfileIds = new HashSet<Guid>();

                foreach (var leader in leaders)
                {
                    var developerProfileId =
                        await _userRepository.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
                    if (developerProfileId is null)
                        return ApiResponse.Failure<Guid>(AppError.Validation("Team leader developer profile was not found."));

                    var canSend = speakerId.HasValue && leader.UserId == speakerId.Value;
                    members.Add((null, developerProfileId, canSend, canSend ? "Team Leader" : "Team Leader (view only)"));
                    addedDeveloperProfileIds.Add(developerProfileId.Value);
                }

                if (speakerId.HasValue)
                {
                    var speakerProfileId =
                        await _userRepository.GetDeveloperProfileIdByUserIdAsync(speakerId.Value, ct);
                    if (speakerProfileId is null)
                        return ApiResponse.Failure<Guid>(AppError.Validation("Speaker developer profile was not found."));

                    if (addedDeveloperProfileIds.Add(speakerProfileId.Value))
                        members.Add((null, speakerProfileId, true, "Team Leader"));
                }
            }

            chatRoom = new ChatRoom
            {
                RoomType = RoomType.Proposal,
                Status = ChatRoomStatus.Active,
                ProposalId = proposal.Id,
                TeamId = proposal.TeamId,
                // Keep null: IX_ChatRooms_ProjectId is unique and is claimed by the Project room on hire.
                // API list DTO still exposes projectId via Proposal.ProjectId.
                ProjectId = null,
                Title = $"{project.Title}",
                CreatedByUserId = _currentUser.UserId
            };

            await _chatRoomRepository.AddWithMembersAsync(chatRoom, members, ct);
            notifyMembers = members;

            await _messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoom.Id,
                MessageType = MessageType.System,
                Text = "Discussion started. Accepting a proposal is not a hire — negotiate the Milestone Plan next."
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        if (notifyMembers is not null)
        {
            foreach (var member in notifyMembers)
            {
                if (member.DeveloperProfileId is null)
                    continue;

                var roomId = chatRoom.Id;
                BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
                {
                    DeveloperProfileId = member.DeveloperProfileId,
                    Title = "Discussion started",
                    Body = $"The client started a discussion on your proposal for \"{project.Title}\".",
                    Type = NotificationType.NewChatMessage,
                    ChatRoomId = roomId,
                    ProjectId = project.Id,
                    ProjectProposalId = proposal.Id,
                    ActionUrl = $"/chat?room={roomId}"
                }));
            }
        }

        return ApiResponse.Success(
            chatRoom.Id,
            "Discussion started. Accepting a proposal is not a hire — agree a milestone plan next.");
    }

    /// <summary>Close discussion → Viewed (not Rejected). Reject is deferred until Hire.</summary>
    public async Task<ApiResponse> CloseDiscussionAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var project = await _projectRepository.GetByIdAsync(proposal.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), proposal.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the project's client can close a discussion."));

        if (proposal.Status != ProposalStatus.InDiscussion)
            return ApiResponse.Failure(AppError.Validation("Proposal is not in discussion."));

        if (project.AssignedUserId is not null || project.AssignedTeamId is not null)
            return ApiResponse.Failure(AppError.Validation("Cannot close discussion after hire."));

        await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.Viewed, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Discussion closed. Proposal returned to Viewed.");
    }

    public async Task<ApiResponse> RejectAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var project = await _projectRepository.GetByIdAsync(proposal.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), proposal.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the project's client can reject a proposal."));

        if (proposal.Status is not (ProposalStatus.Pending or ProposalStatus.Viewed or ProposalStatus.InDiscussion))
            return ApiResponse.Failure(AppError.Validation("Only open proposals can be rejected."));

        await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.Rejected, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await NotifyProposalRejectedAsync(
            proposal,
            project,
            $"Your proposal for \"{project.Title}\" was declined by the client.",
            ct);

        return ApiResponse.Success("Proposal rejected successfully.");
    }

    private async Task NotifyProposalRejectedAsync(
        ProjectProposal proposal,
        Project project,
        string body,
        CancellationToken ct)
    {
        const string title = "Proposal rejected";
        var actionUrl = "/developer/manage-work";

        if (proposal.ApplicantType == ApplicantType.User && proposal.UserId.HasValue)
        {
            var developerProfileId =
                await _userRepository.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
            if (developerProfileId is null)
                return;

            BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
            {
                DeveloperProfileId = developerProfileId.Value,
                Title = title,
                Body = body,
                Type = NotificationType.ProposalRejected,
                ProjectId = project.Id,
                ProjectProposalId = proposal.Id,
                ActionUrl = actionUrl
            }));
            return;
        }

        if (proposal.ApplicantType != ApplicantType.Team || !proposal.TeamId.HasValue)
            return;

        var leaders = await _teamMemberRepository.GetLeadersAsync(proposal.TeamId.Value, ct);
        var notified = new HashSet<Guid>();

        foreach (var leader in leaders)
        {
            var developerProfileId =
                await _userRepository.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
            if (developerProfileId is null || !notified.Add(developerProfileId.Value))
                continue;

            BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
            {
                DeveloperProfileId = developerProfileId.Value,
                Title = title,
                Body = body,
                Type = NotificationType.ProposalRejected,
                ProjectId = project.Id,
                ProjectProposalId = proposal.Id,
                TeamId = proposal.TeamId,
                ActionUrl = actionUrl
            }));
        }

        if (!proposal.UserId.HasValue)
            return;

        var speakerProfileId =
            await _userRepository.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
        if (speakerProfileId is null || !notified.Add(speakerProfileId.Value))
            return;

        BackgroundJob.Enqueue(() => _notificationService.CreateNotification(new CreateNotificationRequest
        {
            DeveloperProfileId = speakerProfileId.Value,
            Title = title,
            Body = body,
            Type = NotificationType.ProposalRejected,
            ProjectId = project.Id,
            ProjectProposalId = proposal.Id,
            TeamId = proposal.TeamId,
            ActionUrl = actionUrl
        }));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(id, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), id));

        _proposalRepository.Delete(proposal);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Proposal deleted successfully.");
    }
}
