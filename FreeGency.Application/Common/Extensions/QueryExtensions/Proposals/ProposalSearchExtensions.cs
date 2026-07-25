using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Proposals;

public static class ProposalSearchExtensions
{
    public static IQueryable<ProjectProposal> ApplySearch(this IQueryable<ProjectProposal> query, FilterProposalDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();

            query = query.Where(p =>
                p.CoverLetter.ToLower().Contains(term) ||
                (p.User != null && (p.User.FristName.ToLower().Contains(term) ||
                p.User.LastName.ToLower().Contains(term))) ||
                (p.Team != null && p.Team.Name.ToLower().Contains(term)));
        }
        
        return query;
    }
}
