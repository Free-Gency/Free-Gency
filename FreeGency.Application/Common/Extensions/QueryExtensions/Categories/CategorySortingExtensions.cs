using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Categories;

public static class CategorySortingExtensions
{
    public static IQueryable<Category> ApplySorting(
        this IQueryable<Category> query,
        FilterCategoriesRequestDto filter)
    {
        var desc = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return filter.SortBy.ToLowerInvariant() switch
        {
            "nameen" => desc ? query.OrderByDescending(c => c.NameEn) : query.OrderBy(c => c.NameEn),
            "createdat" => desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
        };
    }
}
