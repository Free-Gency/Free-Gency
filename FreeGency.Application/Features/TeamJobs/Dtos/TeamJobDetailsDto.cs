using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Features.TeamJobs.Dtos;

public sealed class TeamJobDetailsDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public IEnumerable<SkillDto> Skills { get; set; } = [];
}
