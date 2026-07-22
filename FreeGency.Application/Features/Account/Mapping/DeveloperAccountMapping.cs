using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Features.Account.Mapping;

public static class DeveloperAccountMapping
{
    public static DeveloperAccountResponseDto ToDto(this DeveloperProfile developerProfile)
    {
        return new DeveloperAccountResponseDto
        {
            Id = developerProfile.Id,
            FirstName = developerProfile.User.FristName,
            LastName = developerProfile.User.LastName,
            ProfileImage = developerProfile.ProfileImage,
            Bio = developerProfile.Bio,
            AverageRating = developerProfile.AverageRating,
            RatingCount = developerProfile.RatingCount,
            Country = developerProfile.User.Country ?? string.Empty,
            Specialties = developerProfile.UserSpecialties
                .Select(us => new SpecialtyWithSkillsDto
                {
                    Id = us.Specialty.Id,
                    NameAr = us.Specialty.NameAr,
                    NameEn = us.Specialty.NameEn,
                    Skills = us.Specialty.SpecialtySkills
                        .Select(ss => new SkillDto
                        {
                            Id = ss.Skill.Id,
                            Name = ss.Skill.Name
                        })
                        .ToList()
                })
                .ToList(),
            Skills = developerProfile.UserSkills
                .Select(us => new SkillDto
                {
                    Id = us.Skill.Id,
                    Name = us.Skill.Name
                })
                .ToList()
        };
    }
}
