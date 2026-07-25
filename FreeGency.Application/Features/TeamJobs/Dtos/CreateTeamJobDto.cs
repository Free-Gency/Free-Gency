namespace FreeGency.Application.Features.TeamJobs.Dtos;

public sealed class CreateTeamJobDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<Guid> SkillIds { get; set; } = [];
}
