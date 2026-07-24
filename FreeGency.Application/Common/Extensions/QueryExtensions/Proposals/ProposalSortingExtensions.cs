using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Proposals;

public static class ProposalSortingExtensions
{
    public static IQueryable<ProjectProposal> ApplySorting(this IQueryable<ProjectProposal> query, FilterProposalDto request)
    {
        query = request.SortBy.ToLower() switch
        {
            "appliedat" =>
                request.SortDirection == "asc"
                    ? query.OrderBy(p => p.AppliedAt)
                    : query.OrderByDescending(p => p.AppliedAt),

            "proposedbudget" =>
                request.SortDirection == "asc"
                    ? query.OrderBy(p => p.ProposedBudget)
                    : query.OrderByDescending(p => p.ProposedBudget),

            _ =>
                request.SortDirection == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt)
        };

        return query;
    }
}
