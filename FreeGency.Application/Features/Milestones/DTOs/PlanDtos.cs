namespace FreeGency.Application.Features.Milestones.DTOs;

public sealed class ProposeMilestoneItemDto
{
    public string Title { get; init; } = string.Empty;
    public string DefinitionOfDone { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime? DueDate { get; init; }
}

public sealed class ProposeMilestonePlanDto
{
    public Guid ProjectId { get; init; }
    public Guid ProposalId { get; init; }
    public IReadOnlyList<ProposeMilestoneItemDto> Milestones { get; init; } = [];
}

public sealed class RequestPlanChangesDto
{
    public Guid PlanVersionId { get; init; }
    public string Comment { get; init; } = string.Empty;
}

public sealed class MilestonePlanItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string DefinitionOfDone { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime? DueDate { get; init; }
    public int SortOrder { get; init; }
    public string? ChangeTag { get; init; }
}

public sealed class MilestonePlanVersionDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid ProposalId { get; init; }
    public int Version { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ChangeComment { get; init; }
    public Guid ProposedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<MilestonePlanItemDto> Items { get; init; } = [];
}
