using FreeGency.Application.Features.Proposals.Dtos;
using FreeGency.Domain.Interfaces.Repositories.Teams;

namespace FreeGency.Application.Features.Proposals.Commands;

public partial class ProposalService : IProposalService
{
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProposalService(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _proposalRepository = _unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _projectRepository = _unitOfWork.Repository<IProjectRepository, Project>();
        _teamMemberRepository = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _chatRoomRepository = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
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

        if (await _proposalRepository.HasPendingOrActiveAsync(dto.ProjectId, dto.ApplicantType, applicantId, ct))
            return ApiResponse.Failure(AppError.Validation("You already have a pending or active proposal for this project."));

        var proposal = new ProjectProposal
        {
            ProjectId = dto.ProjectId,
            ApplicantType = dto.ApplicantType,
            TeamId = dto.ApplicantType == ApplicantType.Team ? dto.TeamId : null,
            UserId = dto.ApplicantType == ApplicantType.User ? _currentUser.UserId : null,
            CoverLetter = dto.CoverLetter,
            Approach = dto.Approach ?? string.Empty,
            ProposedTimeline = dto.ProposedTimeline,
            SimilarLinksUrl = dto.SimilarLinksUrl,
            ProposedBudget = dto.ProposedBudget,
            Status = ProposalStatus.Pending
        };

        var attachments = dto.AttachmentUrls
            .Select(url => new ProposalAttachment { FileUrl = url })
            .ToList();

        await _proposalRepository.AddWithAttachmentsAsync(proposal, attachments, ct);
        await _unitOfWork.SaveChangesAsync(ct);

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
    /// </summary>
    public async Task<ApiResponse> StartDiscussionAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var project = await _projectRepository.GetByIdAsync(proposal.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), proposal.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the project's client can start a discussion."));

        if (project.Status != ProjectStatus.Open)
            return ApiResponse.Failure(AppError.Validation("Project is not open for new discussions."));

        if (proposal.Status is ProposalStatus.Rejected or ProposalStatus.Withdrawn or ProposalStatus.Expired)
            return ApiResponse.Failure(AppError.Validation("Cannot discuss a closed proposal."));

        if (proposal.Status == ProposalStatus.InDiscussion)
            return ApiResponse.Success("Discussion is already active for this proposal.");

        var active = (await _proposalRepository.GetActiveDiscussionByProjectIdAsync(proposal.ProjectId, ct)).ToList();
        if (active.Any(p => p.Id != proposalId))
            return ApiResponse.Failure(AppError.Validation(
                "Another discussion is already active. Close it before starting a new one."));

        await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.InDiscussion, ct);

        // Chat: create/reuse room for this proposal only (no ProjectId — unique index + later reopen).
        var existingRoom = await _chatRoomRepository.GetByProposalIdAsync(proposalId, ct);
        if (existingRoom is null)
        {
            var memberUserIds = new List<Guid> { project.ClientId };
            if (proposal.ApplicantType == ApplicantType.User && proposal.UserId.HasValue)
                memberUserIds.Add(proposal.UserId.Value);

            var chatRoom = new ChatRoom
            {
                RoomType = RoomType.Proposal,
                ProposalId = proposal.Id,
                ProjectId = null,
                Title = $"Proposal - {project.Title}",
                CreatedByUserId = _currentUser.UserId
            };

            await _chatRoomRepository.AddWithMembersAsync(chatRoom, memberUserIds, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Discussion started. Accepting a proposal is not a hire — agree a milestone plan next.");
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

        return ApiResponse.Success("Proposal rejected successfully.");
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
