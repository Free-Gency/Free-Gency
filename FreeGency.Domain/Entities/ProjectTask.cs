
namespace FreeGency.Domain.Entities;

public class ProjectTask : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;
    public Guid? AssigneeUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal SpentHours { get; set; } = 0;
    public DateTime? CompletedAt { get; set; }

    public virtual Milestone Milestone { get; set; } = null!;
    public virtual User? Assignee { get; set; }
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<TaskComment> Comments { get; set; } = [];
    public virtual ICollection<TaskChecklistItem> ChecklistItems { get; set; } = [];
    public virtual ICollection<TaskAttachment> Attachments { get; set; } = [];
    public virtual ICollection<TaskTimeLog> TimeLogs { get; set; } = [];
    public virtual ICollection<TaskSubtask> Subtasks { get; set; } = [];
}
