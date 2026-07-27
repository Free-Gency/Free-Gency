
namespace FreeGency.Application.Features.ProjectEvents.DTOs;

public sealed class ProjectEventDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? MilestoneId { get; init; }
    public string? MilestoneTitle { get; init; }
    public Guid ActorUserId { get; init; }
    public string ActorUserName { get; init; } = string.Empty;
    public string EventType { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
