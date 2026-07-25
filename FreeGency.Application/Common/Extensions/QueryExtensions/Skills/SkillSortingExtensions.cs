using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Skills;

public static class SkillSortingExtensions
{
    public static IQueryable<Skill> ApplySorting(
        this IQueryable<Skill> query,
        FilterSkillsRequestDto filter)
    {
        var desc = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return filter.SortBy.ToLowerInvariant() switch
        {
            "createdat" => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => desc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
        };
    }
}
