
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

    public async Task<ApiResponse<ProposalDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(id, ct);
        if (proposal is null)
            return ApiResponse.Failure<ProposalDto>(AppError.NotFound(nameof(ProjectProposal), id));

        return ApiResponse.Success(_mapper.Map<ProposalDto>(proposal));
    }

}