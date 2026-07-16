using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class ProjectMember : ISoftDeletableEntity
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
    public Guid UserId { get; set; }
    public string? RoleInProject { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
