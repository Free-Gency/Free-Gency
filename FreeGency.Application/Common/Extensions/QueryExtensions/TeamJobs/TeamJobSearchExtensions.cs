namespace FreeGency.Application.Common.Extensions.QueryExtensions.TeamJobs;

public static class TeamJobSearchExtensions
{
    public static IQueryable<TeamJob> ApplySearch(this IQueryable<TeamJob> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var keyword = search.Trim();

        return query.Where(j =>
            j.Title.Contains(keyword) ||
            j.Description.Contains(keyword));
    }
}
