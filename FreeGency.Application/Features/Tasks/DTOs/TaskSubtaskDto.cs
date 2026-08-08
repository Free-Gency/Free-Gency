
namespace FreeGency.Application.Features.Tasks.DTOs;

public class TaskSubtaskDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = Domain.Enums.TaskStatus.Todo.ToString();
    public Guid? AssigneeUserId { get; set; }
    public DateTime? DueDate { get; set; }
}


public class CreateSubtaskDto
{
    public string Title { get; set; } = string.Empty;
    public Guid? AssigneeUserId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ChangeSubtaskStatusDto
{
    public Domain.Enums.TaskStatus Status { get; set; }
}
