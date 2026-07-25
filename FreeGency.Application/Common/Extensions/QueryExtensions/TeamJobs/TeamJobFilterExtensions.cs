using FreeGency.Application.Features.TeamJobs.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.TeamJobs;

public static class TeamJobFilterExtensions
{
    public static IQueryable<TeamJob> ApplyFilters(
        this IQueryable<TeamJob> query,
        FilterTeamJobsRequestDto filter)
    {
        if (filter.TeamId.HasValue)
        {
            var teamId = filter.TeamId.Value;
            query = query.Where(j => j.TeamId == teamId);
        }

        return query;
    }
}
