using FreeGency.Application.Features.Escrow.DTOs;
using FreeGency.Domain.Interfaces.Repositories.Teams;

namespace FreeGency.Application.Features.Escrow.Commands;

public partial class EscrowService
{
    public async Task<ApiResponse<EscrowDto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<EscrowDto>(AppError.NotFound(nameof(Project), projectId));

        if (!await CanAccessProjectAsync(project, ct))
            return ApiResponse.Failure<EscrowDto>(
                AppError.Forbidden("You do not have access to this project's escrow."));

        var escrow = await _escrowRepo.GetByProjectIdAsync(projectId, ct);
        if (escrow is null)
            return ApiResponse.Failure<EscrowDto>(AppError.NotFound("EscrowHold", projectId));

        return ApiResponse.Success(_mapper.Map<EscrowDto>(escrow));
    }

    private async Task<bool> CanAccessProjectAsync(Project project, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        if (project.ClientId == userId)
            return true;

        if (project.AssignedUserId == userId)
            return true;

        var teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();

        if (project.AssignedTeamId is not null &&
            await teamMemberRepo.IsMemberAsync(project.AssignedTeamId.Value, userId, ct))
            return true;

        var proposalRepo = _unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        var discussions = await proposalRepo.GetActiveDiscussionByProjectIdAsync(project.Id, ct);
        foreach (var proposal in discussions)
        {
            if (proposal.ApplicantType == ApplicantType.User && proposal.UserId == userId)
                return true;

            if (proposal.ApplicantType == ApplicantType.Team &&
                proposal.TeamId is not null &&
                await teamMemberRepo.IsMemberAsync(proposal.TeamId.Value, userId, ct))
                return true;
        }

        return false;
    }
}
