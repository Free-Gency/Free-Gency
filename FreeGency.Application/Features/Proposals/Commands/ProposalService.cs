
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


    // Command methods
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

        if (proposal.Status != ProposalStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Only pending proposals can be edited."));

        if (dto.CoverLetter is not null)
            proposal.CoverLetter = dto.CoverLetter;

        if (dto.ProposedBudget.HasValue)
            proposal.ProposedBudget = dto.ProposedBudget.Value;

        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Proposal updated successfully.");
    }

    public async Task<ApiResponse<bool>> WithdrawAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure<bool>(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var isOwner = proposal.ApplicantType == ApplicantType.User
            ? proposal.UserId == _currentUser.UserId
            : proposal.TeamId is not null &&
                await _teamMemberRepository.IsLeaderAsync(proposal.TeamId.Value, _currentUser.UserId, ct);

        if (!isOwner)
            return ApiResponse.Failure<bool>(AppError.Forbidden("You are not allowed to withdraw this proposal."));

        if (proposal.Status != ProposalStatus.Pending)
            return ApiResponse.Failure<bool>(AppError.Validation("Only pending proposals can be withdrawn."));

        await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.Withdrawn, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(true, "Proposal withdrawn successfully.");
    }

    public async Task<ApiResponse> AcceptAsync(Guid proposalId, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectProposal), proposalId));

        var project = await _projectRepository.GetByIdAsync(proposal.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), proposal.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the project's client can accept a proposal."));

        if (proposal.Status != ProposalStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Only pending proposals can be accepted."));

        await _proposalRepository.UpdateStatusAsync(proposalId, ProposalStatus.Accepted, ct);

        // auto-reject other pending proposals for this project,
        // assign project.AssignedTeamId/AssignedUserId, open a ChatRoom.

        var otherPendingProposals = await _proposalRepository.GetPendingByProjectIdAsync(proposal.ProjectId, ct);
        foreach (var other in otherPendingProposals.Where(p => p.Id != proposalId))
        {
            await _proposalRepository.UpdateStatusAsync(other.Id, ProposalStatus.Rejected, ct);
        }

        var memberUserIds = new List<Guid> { project.ClientId };
        if (proposal.ApplicantType == ApplicantType.User && proposal.UserId.HasValue)
            memberUserIds.Add(proposal.UserId.Value);

        var chatRoom = new ChatRoom
        {
            RoomType = RoomType.Proposal,
            ProposalId = proposal.Id,
            ProjectId = project.Id,
            Title = $"Proposal - {project.Title}",
            CreatedByUserId = _currentUser.UserId
        };

        await _chatRoomRepository.AddWithMembersAsync(chatRoom, memberUserIds, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Proposal accepted successfully.");
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

        if (proposal.Status != ProposalStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Only pending proposals can be rejected."));

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
