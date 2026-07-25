namespace FreeGency.Application.Common.Extensions.QueryExtensions.Categories;

public static class CategorySearchExtensions
{
    public static IQueryable<Category> ApplySearch(this IQueryable<Category> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var keyword = search.Trim();

        return query.Where(c =>
            c.Name.Contains(keyword) ||
            c.NameEn.Contains(keyword));
    }
}
