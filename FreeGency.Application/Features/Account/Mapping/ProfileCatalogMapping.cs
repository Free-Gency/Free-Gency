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
        return interests
            .Select(ui => ui.Category)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .Select(category => new ProfileInterestDto
            {
                Id = category.Id,
                Name = category.Name,
                NameEn = category.NameEn,
                ImageCover = category.ImageCover,
                Specialties = userSpecialties
                    .Where(us => us.Specialty.CategorySpecialties.Any(cs => cs.CategoryId == category.Id))
                    .Select(us => us.Specialty)
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .Select(specialty => new ProfileSpecialtyDto
                    {
                        Id = specialty.Id,
                        NameAr = specialty.NameAr,
                        NameEn = specialty.NameEn,
                        Skills = userSkills
                            .Where(us => us.Skill.SpecialtySkills.Any(ss => ss.SpecialtyId == specialty.Id))
                            .Select(us => us.Skill)
                            .GroupBy(s => s.Id)
                            .Select(g => g.First())
                            .Select(skill => new SkillDto
                            {
                                Id = skill.Id,
                                Name = skill.Name
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();
    }
}
