
namespace FreeGency.Domain.Entities;

public class TaskSubtask : ISoftDeletableEntity
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
    public string Title { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;
    public Guid? AssigneeUserId { get; set; }
    public DateTime? DueDate { get; set; }

    public virtual ProjectTask Task { get; set; } = null!;
}