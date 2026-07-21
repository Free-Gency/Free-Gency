using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class SavedProject : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
}
