using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.TeamJobs.Dtos;
using FreeGency.Domain.Entities;

namespace FreeGency.Application.Common.Mappings.TeamJobsMapping;

public static class TeamJobMapping
{
    public static TeamJobDto ToDto(this TeamJob teamJob)
    {
        return new TeamJobDto
        {
            Id = teamJob.Id,
            TeamId = teamJob.TeamId,
            Title = teamJob.Title,
            Description = teamJob.Description,
            Status = teamJob.Status.ToString(),
            CreatedAt = teamJob.CreatedAt,
            TeamName = teamJob.Team?.Name ?? string.Empty,
            TeamLogo = teamJob.Team?.Logo,
        };
    }

    public static TeamJobDetailsDto ToDetailsDto(this TeamJob teamJob)
    {
        return new TeamJobDetailsDto
        {
            Id = teamJob.Id,
            TeamId = teamJob.TeamId,
            Title = teamJob.Title,
            Description = teamJob.Description,
            Status = teamJob.Status.ToString(),
            CreatedAt = teamJob.CreatedAt,
            ClosedAt = teamJob.ClosedAt,
            TeamName = teamJob.Team?.Name ?? string.Empty,
            Skills = teamJob.TeamJobSkills?
                .Select(tjs => new SkillDto
                {
                    Id = tjs.Skill.Id,
                    Name = tjs.Skill.Name
                }) ?? []
        };
    }

    public static TeamJob ToEntity(this CreateTeamJobDto dto, Guid teamId, Guid createdByUserId)
    {
        return new TeamJob
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            Title = dto.Title,
            Description = dto.Description,
            CreatedByUserId = createdByUserId
        };
    }
}
