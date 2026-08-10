namespace FreeGency.Application.Features.Milestones.DTOs;

public sealed class DeveloperMilestoneDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public string ProjectStatus { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal ReleasedAmount { get; init; }
    public int SortOrder { get; init; }
    public string ReleaseStatus { get; init; } = default!;
    public string WorkStatus { get; init; } = default!;
    public DateTime? DueDate { get; init; }
    public bool IsFunded { get; init; }
    public bool IsAssignee { get; set; }
    public bool CanSubmit { get; set; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
