namespace FreeGency.Application.Features.TeamJobs.Dtos;

public sealed class UpdateTeamJobDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
