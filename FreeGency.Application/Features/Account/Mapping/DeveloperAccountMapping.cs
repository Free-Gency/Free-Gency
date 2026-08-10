using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Domain.Entities;

namespace FreeGency.Application.Features.Account.Mapping;

public static class DeveloperAccountMapping
{
    public static DeveloperAccountResponseDto ToDto(this DeveloperProfile developerProfile)
    {
        var interests = ProfileCatalogMapping.ToNestedCatalog(
            developerProfile.UserInterests,
            developerProfile.UserSpecialties,
            developerProfile.UserSkills);

        var title = interests
            .SelectMany(i => i.Specialties)
            .Select(s => !string.IsNullOrWhiteSpace(s.NameEn) ? s.NameEn : s.NameAr)
            .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));

        // No dedicated job-success metric yet — map rating (0–5) onto a 0–100 scale.
        var jobSuccess = developerProfile.AverageRating <= 0
            ? 0
            : (int)Math.Round(Math.Clamp((double)developerProfile.AverageRating, 0, 5) / 5d * 100d);

        return new DeveloperAccountResponseDto
        {
            Id = developerProfile.Id,
            UserId = developerProfile.UserId,
            FirstName = developerProfile.User.FristName,
            LastName = developerProfile.User.LastName,
            ProfileImage = developerProfile.ProfileImage,
            Bio = developerProfile.Bio,
            AverageRating = developerProfile.AverageRating,
            RatingCount = developerProfile.RatingCount,
            Country = developerProfile.User.Country ?? string.Empty,
            Title = title,
            IsAvailable = true,
            JobSuccessRate = jobSuccess,
            Interests = interests
        };
    }

    public static void UpdateToEntity(this DeveloperProfile developer, UpdateDeveloperAccountDto dto)
    {
        developer.User.FristName = dto.FirstName;
        developer.User.LastName = dto.LastName;
        if (dto.Country != null)
        {
            developer.User.Country = dto.Country;
        }
        developer.Bio = dto.Bio;
    }
}
