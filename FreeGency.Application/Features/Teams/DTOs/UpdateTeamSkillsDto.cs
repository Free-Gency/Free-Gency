namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class UpdateTeamSkillsDto
{
    public List<Guid> SkillIds { get; set; } = [];
}