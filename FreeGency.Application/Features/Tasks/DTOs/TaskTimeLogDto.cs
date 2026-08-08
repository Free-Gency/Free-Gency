
namespace FreeGency.Application.Features.Tasks.DTOs;

public class TaskTimeLogDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public string? Note { get; set; }
    public DateTime WorkDate { get; set; }
}


public class LogTimeDto
{
    public decimal Hours { get; set; }
    public string? Note { get; set; }
    public DateTime? WorkDate { get; set; }
}
