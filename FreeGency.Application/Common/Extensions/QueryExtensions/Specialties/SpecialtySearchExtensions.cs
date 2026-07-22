namespace FreeGency.Application.Common.Extensions.QueryExtensions.Specialties;

public static class SpecialtySearchExtensions
{
    public static IQueryable<Specialty> ApplySearch(this IQueryable<Specialty> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var keyword = search.Trim();

        return query.Where(s =>
            s.NameEn.Contains(keyword) ||
            s.NameAr.Contains(keyword));
    }
}
