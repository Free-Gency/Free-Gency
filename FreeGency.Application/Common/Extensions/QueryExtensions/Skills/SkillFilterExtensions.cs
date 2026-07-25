using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Common.Extensions.QueryExtensions.Skills;

public static class SkillFilterExtensions
{
    public static IQueryable<Skill> ApplyFilters(
        this IQueryable<Skill> query,
        FilterSkillsRequestDto filter)
    {
        if (filter.CategoryId.HasValue)
        {
            var categoryId = filter.CategoryId.Value;
            query = query.Where(skill =>
                skill.SpecialtySkills.Any(ss =>
                    ss.Specialty.CategorySpecialties.Any(cs => cs.CategoryId == categoryId)));
        }

        if (filter.SpecialtyId.HasValue)
        {
            var specialtyId = filter.SpecialtyId.Value;
            query = query.Where(skill =>
                skill.SpecialtySkills.Any(ss => ss.SpecialtyId == specialtyId));
        }

        return query;
    }
}
