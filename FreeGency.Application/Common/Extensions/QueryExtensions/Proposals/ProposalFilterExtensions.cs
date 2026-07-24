using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Proposals;

public static class ProposalFilterExtensions
{
    public static IQueryable<ProjectProposal> ApplyFilters(this IQueryable<ProjectProposal> query, FilterProposalDto request)
    {
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status);

        if (request.ProjectId.HasValue)
            query = query.Where(p => p.ProjectId == request.ProjectId);

        if (request.UserId.HasValue)
            query = query.Where(p => p.UserId == request.UserId);

        if (request.TeamId.HasValue)
            query = query.Where(p => p.TeamId == request.TeamId);

        if (request.ApplicantType.HasValue)
            query = query.Where(p => p.ApplicantType == request.ApplicantType);

        if (request.BudgetMin.HasValue)
            query = query.Where(p => p.ProposedBudget >= request.BudgetMin.Value);

        if (request.BudgetMax.HasValue)
            query = query.Where(p => p.ProposedBudget <= request.BudgetMax.Value);


        return query;
    }
}
