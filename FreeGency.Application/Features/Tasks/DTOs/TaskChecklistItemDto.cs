
namespace FreeGency.Application.Features.Tasks.DTOs;

public class TaskChecklistItemDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

}


public class CreateChecklistItemDto
{
    public string Title { get; set; } = string.Empty;
}


public class ToggleChecklistItemDto
{
    public bool IsCompleted { get; set; }
}
