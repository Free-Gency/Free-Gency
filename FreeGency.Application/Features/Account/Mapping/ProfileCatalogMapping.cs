using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Domain.Entities;

namespace FreeGency.Application.Features.Account.Mapping;

public static class ProfileCatalogMapping
{
    public static List<ProfileInterestDto> ToNestedCatalog(
        IEnumerable<UserInterest> interests,
        IEnumerable<UserSpecialty> userSpecialties,
        IEnumerable<UserSkill> userSkills)
    {
        var specialtyList = userSpecialties?.ToList() ?? [];
        var skillList = userSkills?.ToList() ?? [];

        var catalog = interests
            .Select(ui => ui.Category)
            .Where(c => c is not null)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .Select(category => new ProfileInterestDto
            {
                Id = category.Id,
                Name = category.Name,
                NameEn = category.NameEn,
                ImageCover = category.ImageCover,
                Specialties = MapSpecialtiesForCategory(category.Id, specialtyList, skillList)
            })
            .ToList();

        // If category↔specialty joins weren't loaded, still surface specialties on the first category.
        if (catalog.Count > 0
            && catalog.All(c => c.Specialties.Count == 0)
            && specialtyList.Count > 0)
        {
            catalog[0].Specialties = specialtyList
                .Select(us => us.Specialty)
                .Where(s => s is not null)
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .Select(specialty => MapSpecialty(specialty, skillList, allowUnlinkedSkills: false))
                .ToList();

            // Attach orphan skills under the first specialty when join tables are empty.
            if (catalog[0].Specialties.Count > 0
                && catalog[0].Specialties.All(s => s.Skills.Count == 0)
                && skillList.Count > 0)
            {
                catalog[0].Specialties[0].Skills = skillList
                    .Select(us => us.Skill)
                    .Where(s => s is not null)
                    .GroupBy(s => s!.Id)
                    .Select(g => g.First()!)
                    .Select(skill => new SkillDto { Id = skill.Id, Name = skill.Name })
                    .ToList();
            }
        }

        return catalog;
    }

    private static List<ProfileSpecialtyDto> MapSpecialtiesForCategory(
        Guid categoryId,
        List<UserSpecialty> specialtyList,
        List<UserSkill> skillList)
    {
        return specialtyList
            .Where(us =>
                us.Specialty?.CategorySpecialties != null
                && us.Specialty.CategorySpecialties.Any(cs => cs.CategoryId == categoryId))
            .Select(us => us.Specialty)
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .Select(specialty => MapSpecialty(specialty, skillList, allowUnlinkedSkills: false))
            .ToList();
    }

    private static ProfileSpecialtyDto MapSpecialty(
        Specialty specialty,
        List<UserSkill> skillList,
        bool allowUnlinkedSkills)
    {
        var skills = skillList
            .Where(us =>
                us.Skill?.SpecialtySkills != null
                && us.Skill.SpecialtySkills.Any(ss => ss.SpecialtyId == specialty.Id))
            .Select(us => us.Skill)
            .GroupBy(s => s!.Id)
            .Select(g => g.First()!)
            .Select(skill => new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            })
            .ToList();

        if (allowUnlinkedSkills && skills.Count == 0)
        {
            skills = skillList
                .Select(us => us.Skill)
                .Where(s => s is not null)
                .GroupBy(s => s!.Id)
                .Select(g => g.First()!)
                .Select(skill => new SkillDto { Id = skill.Id, Name = skill.Name })
                .ToList();
        }

        return new ProfileSpecialtyDto
        {
            Id = specialty.Id,
            NameAr = specialty.NameAr,
            NameEn = specialty.NameEn,
            Skills = skills
        };
    }
}
