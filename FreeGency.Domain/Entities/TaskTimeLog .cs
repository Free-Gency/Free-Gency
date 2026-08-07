
namespace FreeGency.Domain.Entities;

public class TaskTimeLog : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public decimal Hours { get; set; }
    public string? Note { get; set; }
    public DateTime WorkDate { get; set; }

    public virtual ProjectTask Task { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
