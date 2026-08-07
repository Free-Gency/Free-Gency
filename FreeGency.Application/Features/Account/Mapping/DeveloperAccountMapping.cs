using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Domain.Entities;

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
            Interests = ProfileCatalogMapping.ToNestedCatalog(
                developerProfile.UserInterests,
                developerProfile.UserSpecialties,
                developerProfile.UserSkills)
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
