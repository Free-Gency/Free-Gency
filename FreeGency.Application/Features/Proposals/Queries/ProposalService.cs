
using FreeGency.Application.Common.Extensions.QueryExtensions.Proposals;
using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Features.Proposals.Commands;

public partial class ProposalService
{
    // Query methods
    public async Task<ApiResponse<PaginatedResult<ProposalDto>>> BrowseAsync(FilterProposalDto filter, CancellationToken ct = default)
    {
        var proposalsQuery = _proposalRepository.Query();

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

        return ApiResponse.Success(_mapper.Map<ProposalDto>(proposal));
    }

}