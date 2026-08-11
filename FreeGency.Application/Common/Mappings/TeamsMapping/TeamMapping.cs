using FreeGency.Application.Features.Teams.Dtos;


namespace FreeGency.Application.Common.Mappings.TeamsMapping;

public static class TeamMapping
{
    public static TeamDto ToDto(this Team team, Guid? currentUserId = null)
    {
        var membersCount = team.TeamMembers?.Count ?? 0;

        return new()
        {
            Id = team.Id,
            Name = team.Name,
            Logo = team.Logo,
            Cover = team.Cover,
            TeamCode = team.TeamCode,
            AboutUs = team.AboutUs,
            AverageRating = team.AverageRating,
            RatingCount = team.RatingCount,
            OwnerUserId = team.OwnerUserId,
            OwnerName = $"{team.Owner?.FristName} {team.Owner?.LastName}".Trim(),
            MembersCount = membersCount,
            MyRole = ResolveMyRole(team, currentUserId),
            MemberAvatars = BuildMemberAvatars(team),
            ProjectsCount = team.AssignedProjects?.Count ?? 0,
            Categories = team.TeamCategories?.Select(tc => new TeamCategoryDto
            {
                CategoryId = tc.CategoryId,
                Name = tc.Category?.Name ?? string.Empty,
                NameEn = tc.Category?.NameEn ?? string.Empty,
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
    }

    public static TeamDto ToDto(this TeamHubItem item)
        => new()
        {
            Id = item.Id,
            Name = item.Name,
            Logo = item.Logo,
            Cover = item.Cover,
            TeamCode = item.TeamCode,
            AboutUs = item.AboutUs,
            AverageRating = item.AverageRating,
            RatingCount = item.RatingCount,
            OwnerUserId = item.OwnerUserId,
            OwnerName = item.OwnerName.Trim(),
            MembersCount = item.MembersCount,
            ProjectsCount = item.ProjectsCount,
            MyRole = item.MyRole,
            MemberAvatars = item.MemberAvatars.Select(a => new TeamMemberAvatarDto
            {
                UserId = a.UserId,
                Name = a.Name,
                ImageUrl = a.ImageUrl
            }).ToList(),
            Categories = item.Categories.Select(c => new TeamCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                NameEn = c.NameEn,
                IsPrimary = c.IsPrimary
            }).ToList(),
            Specialties = item.Specialties.Select(s => new TeamSpecialtyDto
            {
                SpecialtyId = s.SpecialtyId,
                NameEn = s.NameEn,
                NameAr = s.NameAr
            }).ToList(),
            Skills = item.Skills.Select(s => new TeamSkillDto
            {
                SkillId = s.SkillId,
                Name = s.Name
            }).ToList()
        };

    public static Team ToEntity(this CreateTeamDto dto, Guid ownerUserId, string teamCode, string? logoUrl, string? coverUrl = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            AboutUs = dto.AboutUs,
            Logo = logoUrl,
            Cover = coverUrl,
            OwnerUserId = ownerUserId,
            TeamCode = teamCode
        };

    private static string? ResolveMyRole(Team team, Guid? currentUserId)
    {
        if (currentUserId is null || currentUserId == Guid.Empty)
            return null;

        if (team.OwnerUserId == currentUserId)
            return nameof(Role.TeamLeader);

        var membership = team.TeamMembers?.FirstOrDefault(tm => tm.UserId == currentUserId);
        return membership is null ? null : membership.TeamRole.ToString();
    }

    private static List<TeamMemberAvatarDto> BuildMemberAvatars(Team team)
    {
        var fromMembers = (team.TeamMembers ?? [])
            .OrderBy(tm => tm.TeamRole == Role.TeamLeader ? 0 : 1)
            .ThenBy(tm => tm.JoinedAt)
            .Select(tm => new TeamMemberAvatarDto
            {
                UserId = tm.UserId,
                Name = $"{tm.User?.FristName ?? string.Empty} {tm.User?.LastName ?? string.Empty}".Trim(),
                ImageUrl = tm.User?.DeveloperProfile?.ProfileImage
                    ?? tm.User?.ClientProfile?.ProfileImage
            })
            .Where(a => !string.IsNullOrWhiteSpace(a.Name) || a.UserId != Guid.Empty)
            .ToList();

        if (fromMembers.Count > 0)
            return fromMembers;

        if (team.Owner is null && team.OwnerUserId == Guid.Empty)
            return [];

        var ownerName = $"{team.Owner?.FristName ?? string.Empty} {team.Owner?.LastName ?? string.Empty}".Trim();
        if (string.IsNullOrWhiteSpace(ownerName))
            ownerName = "Owner";

        return
        [
            new TeamMemberAvatarDto
            {
                UserId = team.OwnerUserId,
                Name = ownerName,
                ImageUrl = team.Owner?.DeveloperProfile?.ProfileImage
                    ?? team.Owner?.ClientProfile?.ProfileImage
            }
        ];
    }
}
