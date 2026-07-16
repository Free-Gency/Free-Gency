using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class TeamJob : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid TeamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TeamJobStatus Status { get; set; } = TeamJobStatus.open;
    public Guid CreatedByUserId { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Team Team { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<TeamJobSkill> TeamJobSkills { get; set; } = [];
    public ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = [];
}
