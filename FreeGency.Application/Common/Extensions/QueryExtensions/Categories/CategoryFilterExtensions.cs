using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Categories;

public static class CategoryFilterExtensions
{
    public static IQueryable<Category> ApplyFilters(
        this IQueryable<Category> query,
        FilterCategoriesRequestDto filter)
        => query;
}
