using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Domain.Entities;

namespace FreeGency.Application.Features.Account.Mapping;

public static class ClientAccountMapping
{
    public static ClientAccountResponseDto ToDto(this ClientProfile clientProfile)
    {
        return new ClientAccountResponseDto
        {
            UserId = clientProfile.UserId,
            FirstName = clientProfile.User.FristName,
            LastName = clientProfile.User.LastName,
            RatingCount = clientProfile.RatingCount,
            AverageRating = clientProfile.AverageRating,
            Bio = clientProfile.Bio,
            Country = clientProfile.User.Country,
            ProfileImage = clientProfile.ProfileImage,
            IsVerified = clientProfile.User.IsVerified,
            ProjectsCompletedCount = 0, // filled in GetClientProfile from live project counts
            TotalSpent = 0,
            JoinedAt = clientProfile.User.CreatedAt,
            Email = clientProfile.User.Email!,
            ProjectsPostedCount = 0, // filled in GetClientProfile from live project counts
            ProfileMode = clientProfile.User.ActiveProfileMode.ToString(),
            // Interests tree is loaded via GET client/me/interests — keep profile payload light.
            Interests = [],
        };
    }

    public static List<ProfileInterestDto> ToInterestCatalog(this ClientProfile clientProfile)
    {
        return ProfileCatalogMapping.ToNestedCatalog(
            clientProfile.UserInterests ?? [],
            clientProfile.UserSpecialties ?? [],
            clientProfile.UserSkills ?? []);
    }

    public static void UpdateToEntity(this ClientProfile client, UpdateClientAccountDto dto)
    {
        client.User.FristName = dto.FirstName;
        client.User.LastName = dto.LastName;
        if (dto.Country != null)
        {
            client.User.Country = dto.Country;
        }
        client.Bio = dto.Bio;
    }
}
