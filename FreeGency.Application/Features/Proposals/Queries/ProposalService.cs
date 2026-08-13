
using FreeGency.Application.Common.Extensions.QueryExtensions.Proposals;
using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Features.Proposals.Commands;

public partial class ProposalService
{
    // Query methods
    public async Task<ApiResponse<PaginatedResult<ProposalDto>>> BrowseAsync(FilterProposalDto filter, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        // Client Manage Work: only proposals on projects the current user owns.
        var proposalsQuery = _proposalRepository.Query()
            .Where(p => p.Project.ClientId == userId);

        if (filter.ProjectId.HasValue)
        {
            var ownsProject = await _projectRepository.Query()
                .AnyAsync(p => p.Id == filter.ProjectId.Value && p.ClientId == userId, ct);
            if (!ownsProject)
                return ApiResponse.Failure<PaginatedResult<ProposalDto>>(
                    AppError.Forbidden("You do not own this project."));
        }

        var filteredProposals = proposalsQuery
            .ApplyFilters(filter)
            .ApplySearch(filter)
            .ApplySorting(filter);

        var pagedResult =
            await PaginatedResult<ProposalDto>.CreateAsync(
                filteredProposals.ProjectTo<ProposalDto>(_mapper.ConfigurationProvider),
                filter.PageNumber,
                filter.PageSize,
                ct);

        return ApiResponse.Success(pagedResult);
    }

    public async Task<ApiResponse<PaginatedResult<ProposalDto>>> GetMyProposalsAsync(
        FilterProposalDto filter,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var myProposals = _proposalRepository.Query()
            // Manage Work: personal proposals only. Team applications live under Teams.
            .Where(p => p.UserId == userId && p.TeamId == null)
            .ApplyFilters(filter)
            .ApplySearch(filter)
            .ApplySorting(filter);

        var pagedResult = await PaginatedResult<ProposalDto>.CreateAsync(
            myProposals.ProjectTo<ProposalDto>(_mapper.ConfigurationProvider),
            filter.PageNumber,
            filter.PageSize,
            ct);

        return ApiResponse.Success(pagedResult);
    }

    public async Task<ApiResponse<ProposalDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.Query()
            .Include(p => p.Project)
            .Include(p => p.Team).ThenInclude(t => t!.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(p => p.Team).ThenInclude(t => t!.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(p => p.User).ThenInclude(u => u!.ClientProfile)
            .Include(p => p.User).ThenInclude(u => u!.DeveloperProfile).ThenInclude(dp => dp!.UserSkills).ThenInclude(us => us.Skill)
            .Include(p => p.User).ThenInclude(u => u!.DeveloperProfile).ThenInclude(dp => dp!.UserSpecialties).ThenInclude(us => us.Specialty)
            .Include(p => p.ProposalAttachments)
            .Include(p => p.ChatRoom)
            .Include(p => p.Project).ThenInclude(pr => pr.ChatRooms)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (proposal is null)
            return ApiResponse.Failure<ProposalDto>(AppError.NotFound(nameof(ProjectProposal), id));

        var isClient = proposal.Project.ClientId == _currentUser.UserId;
        var isApplicant = proposal.UserId == _currentUser.UserId;
        var isTeamLeader = proposal.TeamId.HasValue
            && await _teamMemberRepository.IsLeaderAsync(proposal.TeamId.Value, _currentUser.UserId, ct);

        if (!isClient && !isApplicant && !isTeamLeader)
            return ApiResponse.Failure<ProposalDto>(AppError.Forbidden("You cannot view this proposal."));

        return ApiResponse.Success(_mapper.Map<ProposalDto>(proposal));
    }

}