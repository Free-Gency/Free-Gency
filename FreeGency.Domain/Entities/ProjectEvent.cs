using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class ProjectEvent : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public Guid ActorUserId { get; set; }
    public EventType EventType { get; set; }

    public Project Project { get; set; } = null!;
    public Milestone? Milestone { get; set; }
    public User ActorUser { get; set; } = null!;
}
