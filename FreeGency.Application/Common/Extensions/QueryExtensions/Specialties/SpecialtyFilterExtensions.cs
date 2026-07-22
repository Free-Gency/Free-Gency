using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Specialties;

public static class SpecialtyFilterExtensions
{
    public static IQueryable<Specialty> ApplyFilters(
        this IQueryable<Specialty> query,
        FilterSpecialtiesRequestDto filter)
    {
        if (filter.CategoryId.HasValue)
        {
            var categoryId = filter.CategoryId.Value;
            query = query.Where(s => s.CategorySpecialties.Any(cs => cs.CategoryId == categoryId));
        }

        return query;
    }
}
