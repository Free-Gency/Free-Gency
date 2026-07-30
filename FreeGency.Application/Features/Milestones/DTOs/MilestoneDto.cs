
namespace FreeGency.Application.Features.Milestones.DTOs;

public sealed class MilestoneDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal ReleasedAmount { get; init; }
    public int SortOrder { get; init; }
    public string ReleaseStatus { get; init; } = default!;
    public string WorkStatus { get; init; } = default!;
    public Guid? ProposedByUserId { get; init; }
    public DateTime? DueDate { get; init; }
    public bool IsFunded { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? AvailableAt { get; init; }
    public DateTime? ReleasedAt { get; init; }
    public IEnumerable<MilestoneFileDto> Files { get; init; } = [];
}