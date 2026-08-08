
namespace FreeGency.Application.Features.Tasks.DTOs;

public class TaskCommentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


public class CreateTaskCommentDto
{
    public string Content { get; set; } = string.Empty;
}
