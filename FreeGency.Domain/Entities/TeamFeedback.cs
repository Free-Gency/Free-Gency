using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class TeamFeedback : ISoftDeletableEntity
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
    public Guid ReviewerUserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public virtual Team Team { get; set; } = null!;
    public virtual User ReviewerUser { get; set; } = null!;
}
