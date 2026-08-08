
namespace FreeGency.Application.Features.Tasks.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    public Guid ProjectId { get; set; }
    public string MilestoneTitle { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string Priority { get; set; } = TaskPriority.Medium.ToString();
    public string Status { get; set; } = Domain.Enums.TaskStatus.Todo.ToString();
    public Guid? AssigneeUserId { get; set; }
    public string? AssigneeName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal SpentHours { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanManage { get; set; }
    public int IncompleteSubtasksCount { get; set; }

    public List<TaskCommentDto> Comments { get; set; } = [];
    public List<TaskChecklistItemDto> ChecklistItems { get; set; } = [];
    public List<TaskAttachmentDto> Attachments { get; set; } = [];
    public List<TaskTimeLogDto> TimeLogs { get; set; } = [];
    public List<TaskSubtaskDto> Subtasks { get; set; } = [];
}



public class TaskAssigneeDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Job { get; set; }
}



public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? AssigneeUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
}

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
}


public class ChangeTaskStatusDto
{
    public Domain.Enums.TaskStatus Status { get; set; }
}


public class AssignTaskDto
{
    public Guid? AssigneeUserId { get; set; }
}