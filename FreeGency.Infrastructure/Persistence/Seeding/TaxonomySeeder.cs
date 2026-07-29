using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Seeding;

public static class TaxonomySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken ct = default)
    {
        var seededAt = DateTime.UtcNow;

        var categoriesByName = (await context.Categories.ToListAsync(ct))
            .GroupBy(c => c.NameEn, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var specialtiesByName = (await context.Specialties.ToListAsync(ct))
            .GroupBy(s => s.NameEn, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var skillsByName = (await context.Skills.ToListAsync(ct))
            .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var existingCategorySpecialtyKeys = (await context.CategorySpecialties
                .Select(cs => new { cs.CategoryId, cs.SpecialtyId })
                .ToListAsync(ct))
            .Select(x => (x.CategoryId, x.SpecialtyId))
            .ToHashSet();

        var existingSpecialtySkillKeys = (await context.SpecialtySkills
                .Select(ss => new { ss.SpecialtyId, ss.SkillId })
                .ToListAsync(ct))
            .Select(x => (x.SpecialtyId, x.SkillId))
            .ToHashSet();

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

        foreach (var def in TaxonomySeedData.Categories)
        {
            if (categoriesByName.ContainsKey(def.NameEn))
                continue;

            var category = new Category
            {
                Id = TaxonomySeedData.CategoryId(def.Key),
                Name = def.NameAr,
                NameEn = def.NameEn,
                CreatedAt = seededAt,
                CreatedBy = TaxonomySeedData.SeedUser
            };
            await context.Categories.AddAsync(category, ct);
            categoriesByName[def.NameEn] = category;
        }

        foreach (var nameEn in specialtyNames)
        {
            if (specialtiesByName.ContainsKey(nameEn))
                continue;

            var specialty = new Specialty
            {
                Id = specialtyIds[nameEn],
                NameEn = nameEn,
                NameAr = TaxonomySeedData.SpecialtyNamesAr.TryGetValue(nameEn, out var nameAr)
                    ? nameAr
                    : nameEn,
                CreatedAt = seededAt,
                CreatedBy = TaxonomySeedData.SeedUser
            };
            await context.Specialties.AddAsync(specialty, ct);
            specialtiesByName[nameEn] = specialty;
        }

        foreach (var name in skillNames)
        {
            if (skillsByName.ContainsKey(name))
                continue;

            var skill = new Skill
            {
                Id = skillIds[name],
                Name = name,
                CreatedAt = seededAt,
                CreatedBy = TaxonomySeedData.SeedUser
            };
            await context.Skills.AddAsync(skill, ct);
            skillsByName[name] = skill;
        }

        var categorySpecialties = new List<CategorySpecialty>();
        foreach (var def in TaxonomySeedData.Categories)
        {
            if (!categoriesByName.TryGetValue(def.NameEn, out var category))
                continue;

            foreach (var specialtyName in def.Specialties.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!specialtiesByName.TryGetValue(specialtyName, out var specialty))
                    continue;

                var key = (category.Id, specialty.Id);
                if (!existingCategorySpecialtyKeys.Add(key))
                    continue;

                categorySpecialties.Add(new CategorySpecialty
                {
                    Id = Guid.NewGuid(),
                    CategoryId = category.Id,
                    SpecialtyId = specialty.Id,
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
            if (!specialtiesByName.TryGetValue(specialtyName, out var specialty))
                continue;

            foreach (var skillName in linkedSkills.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                if (!skillsByName.TryGetValue(skillName, out var skill))
                    continue;

                var key = (specialty.Id, skill.Id);
                if (!existingSpecialtySkillKeys.Add(key))
                    continue;

                specialtySkills.Add(new SpecialtySkill
                {
                    Id = Guid.NewGuid(),
                    SpecialtyId = specialty.Id,
                    SkillId = skill.Id,
                    CreatedAt = seededAt,
                    CreatedBy = TaxonomySeedData.SeedUser
                });
            }
        }

        if (categorySpecialties.Count > 0)
            await context.CategorySpecialties.AddRangeAsync(categorySpecialties, ct);

        if (specialtySkills.Count > 0)
            await context.SpecialtySkills.AddRangeAsync(specialtySkills, ct);

        if (context.ChangeTracker.HasChanges())
            await context.SaveChangesAsync(ct);
    }
}
