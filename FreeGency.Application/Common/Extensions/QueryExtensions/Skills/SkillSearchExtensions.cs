namespace FreeGency.Application.Common.Extensions.QueryExtensions.Skills;

public static class SkillSearchExtensions
{
    public static IQueryable<Skill> ApplySearch(this IQueryable<Skill> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var keyword = search.Trim();
        return query.Where(s => s.Name.Contains(keyword));
    }
}
