namespace FreeGency.Application.Features.TeamJobs.Dtos;

public sealed class UpdateTeamJobSkillsDto
{
    public Guid Id { get; set; }
    public IEnumerable<Guid> SkillIds { get; set; } = [];
}
