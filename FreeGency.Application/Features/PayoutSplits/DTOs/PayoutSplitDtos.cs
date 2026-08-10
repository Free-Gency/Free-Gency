namespace FreeGency.Application.Features.PayoutSplits.DTOs;

public sealed class PayoutSplitItemDto
{
    public Guid UserId { get; init; }
    public decimal Value { get; init; }
}

public sealed class PayoutSplitsDto
{
    public Guid TeamId { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? MilestoneId { get; init; }
    public string SplitType { get; init; } = "Percent";
    public IReadOnlyList<PayoutSplitItemDto> Items { get; init; } = [];
}

public sealed class ReplacePayoutSplitsDto
{
    public string SplitType { get; init; } = "Percent";
    public List<PayoutSplitItemDto> Items { get; init; } = [];
}
