using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Specialties;

public static class SpecialtySortingExtensions
{
    public static IQueryable<Specialty> ApplySorting(
        this IQueryable<Specialty> query,
        FilterSpecialtiesRequestDto filter)
    {
        var desc = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return filter.SortBy.ToLowerInvariant() switch
        {
            "namear" => desc ? query.OrderByDescending(s => s.NameAr) : query.OrderBy(s => s.NameAr),
            "createdat" => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => desc ? query.OrderByDescending(s => s.NameEn) : query.OrderBy(s => s.NameEn),
        };
    }
}
