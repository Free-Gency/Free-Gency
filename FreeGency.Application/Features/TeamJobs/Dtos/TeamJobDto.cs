namespace FreeGency.Application.Features.TeamJobs.Dtos;

public sealed class TeamJobDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamLogo { get; set; }
}
