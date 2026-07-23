using FreeGency.Application.Features.TeamJobs.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.TeamJobs;

public static class TeamJobSortingExtensions
{
    public static IQueryable<TeamJob> ApplySorting(
        this IQueryable<TeamJob> query,
        FilterTeamJobsRequestDto filter)
    {
        var desc = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return filter.SortBy.ToLowerInvariant() switch
        {
            "description" => desc ? query.OrderByDescending(j => j.Description) : query.OrderBy(j => j.Description),
            "createdat" => desc ? query.OrderByDescending(j => j.CreatedAt) : query.OrderBy(j => j.CreatedAt),
            _ => desc ? query.OrderByDescending(j => j.Title) : query.OrderBy(j => j.Title),
        };
    }
}
