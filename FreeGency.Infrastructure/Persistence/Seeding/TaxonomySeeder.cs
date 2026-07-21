using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Seeding;

public static class TaxonomySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.Categories.AnyAsync(ct))
            return;

        var seededAt = DateTime.UtcNow;
        var specialtyNames = TaxonomySeedData.Categories
            .SelectMany(c => c.Specialties)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var skillNames = TaxonomySeedData.Categories
            .SelectMany(c => c.Skills)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var specialtyIds = specialtyNames
            .Select((name, index) => (name, Id: TaxonomySeedData.EntityId("22222222-2222-2222-2222", index + 1)))
            .ToDictionary(x => x.name, x => x.Id, StringComparer.OrdinalIgnoreCase);

        var skillIds = skillNames
            .Select((name, index) => (name, Id: TaxonomySeedData.EntityId("33333333-3333-3333-3333", index + 1)))
            .ToDictionary(x => x.name, x => x.Id, StringComparer.OrdinalIgnoreCase);

        var categories = TaxonomySeedData.Categories.Select(def => new Category
        {
            Id = TaxonomySeedData.CategoryId(def.Key),
            Name = def.NameAr,
            NameEn = def.NameEn,
            CreatedAt = seededAt,
            CreatedBy = TaxonomySeedData.SeedUser
        }).ToList();

        var specialties = specialtyNames.Select(nameEn => new Specialty
        {
            Id = specialtyIds[nameEn],
            NameEn = nameEn,
            NameAr = TaxonomySeedData.SpecialtyNamesAr.TryGetValue(nameEn, out var nameAr)
                ? nameAr
                : nameEn,
            CreatedAt = seededAt,
            CreatedBy = TaxonomySeedData.SeedUser
        }).ToList();

        var skills = skillNames.Select(name => new Skill
        {
            Id = skillIds[name],
            Name = name,
            CreatedAt = seededAt,
            CreatedBy = TaxonomySeedData.SeedUser
        }).ToList();

        var categorySpecialties = new List<CategorySpecialty>();
        foreach (var def in TaxonomySeedData.Categories)
        {
            var categoryId = TaxonomySeedData.CategoryId(def.Key);
            foreach (var specialtyName in def.Specialties.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                categorySpecialties.Add(new CategorySpecialty
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    SpecialtyId = specialtyIds[specialtyName],
                    CreatedAt = seededAt,
                    CreatedBy = TaxonomySeedData.SeedUser
                });
            }
        }

        var specialtySkillNames = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var def in TaxonomySeedData.Categories)
        {
            foreach (var specialtyName in def.Specialties)
            {
                if (!specialtySkillNames.TryGetValue(specialtyName, out var skillSet))
                {
                    skillSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    specialtySkillNames[specialtyName] = skillSet;
                }

                foreach (var skillName in def.Skills)
                    skillSet.Add(skillName);
            }
        }

        var specialtySkills = new List<SpecialtySkill>();
        foreach (var (specialtyName, linkedSkills) in specialtySkillNames)
        {
            var specialtyId = specialtyIds[specialtyName];
            foreach (var skillName in linkedSkills.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                specialtySkills.Add(new SpecialtySkill
                {
                    Id = Guid.NewGuid(),
                    SpecialtyId = specialtyId,
                    SkillId = skillIds[skillName],
                    CreatedAt = seededAt,
                    CreatedBy = TaxonomySeedData.SeedUser
                });
            }
        }

        await context.Categories.AddRangeAsync(categories, ct);
        await context.Specialties.AddRangeAsync(specialties, ct);
        await context.Skills.AddRangeAsync(skills, ct);
        await context.CategorySpecialties.AddRangeAsync(categorySpecialties, ct);
        await context.SpecialtySkills.AddRangeAsync(specialtySkills, ct);
        await context.SaveChangesAsync(ct);
    }
}
