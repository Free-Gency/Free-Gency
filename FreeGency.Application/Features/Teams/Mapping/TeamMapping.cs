using FreeGency.Application.Features.Teams.Dtos;

namespace FreeGency.Application.Common.Mappings.TeamsMapping;

public static class TeamMapping
{
    public static TeamDto ToDto(this Team team)
        => new()
        {
            Id = team.Id,
            Name = team.Name,
            Logo = team.Logo,
            TeamCode = team.TeamCode,
            AboutUs = team.AboutUs,
            AverageRating = team.AverageRating,
            RatingCount = team.RatingCount,
            OwnerUserId = team.OwnerUserId,
            OwnerName = team.Owner?.FristName + " " + team.Owner?.LastName,
            MembersCount = team.TeamMembers?.Count ?? 0,
            Categories = team.TeamCategories?.Select(tc => new TeamCategoryDto
            {
                CategoryId = tc.CategoryId,
                Name = tc.Category?.Name ?? string.Empty,
                IsPrimary = tc.IsPrimary
            }).ToList() ?? [],
            Specialties = team.TeamSpecialties?.Select(ts => new TeamSpecialtyDto
            {
                SpecialtyId = ts.SpecialtyId,
                NameEn = ts.Specialty?.NameEn ?? string.Empty,
                NameAr = ts.Specialty?.NameAr ?? string.Empty
            }).ToList() ?? [],
            Skills = team.TeamSkills?.Select(ts => new TeamSkillDto
            {
                SkillId = ts.SkillId,
                Name = ts.Skill?.Name ?? string.Empty
            }).ToList() ?? []
        };

    public static Team ToEntity(this CreateTeamDto dto, Guid ownerUserId, string teamCode, string? logoUrl)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            AboutUs = dto.AboutUs,
            Logo = logoUrl,
            OwnerUserId = ownerUserId,
            TeamCode = teamCode
        };
}