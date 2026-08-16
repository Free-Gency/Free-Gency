namespace FreeGency.Application.Features.Milestones.DTOs;

public enum MilestonePlanAiAssistMode
{
    FullPlan = 0,
    ApplyChangeRequest = 1,
    Milestone = 2,
    Field = 3
}

public sealed class MilestonePlanAiDraftItemDto
{
    public string Title { get; init; } = string.Empty;
    public string DefinitionOfDone { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? DueDate { get; init; }
}

public sealed class MilestonePlanAiAssistRequestDto
{
    public Guid ProposalId { get; init; }
    public MilestonePlanAiAssistMode Mode { get; init; } = MilestonePlanAiAssistMode.FullPlan;
    public IReadOnlyList<MilestonePlanAiDraftItemDto>? CurrentMilestones { get; init; }
    public int? MilestoneIndex { get; init; }
    public string? Field { get; init; }
    public string? ChangeComment { get; init; }
}

public sealed class MilestonePlanAiAssistResponseDto
{
    public IReadOnlyList<MilestonePlanAiDraftItemDto> Milestones { get; init; } = [];
}
