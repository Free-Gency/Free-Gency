namespace FreeGency.AI.HiringAgent;

public sealed class DiscussionAgentReply
{
    public string Message { get; init; } = string.Empty;
    public bool DoneNegotiating { get; init; }
    public string? InternalNote { get; init; }

    /// <summary>When true, open the normal plan revision loop (status → ChangesRequested).</summary>
    public bool RequestPlanChanges { get; init; }

    /// <summary>Required when <see cref="RequestPlanChanges"/> is true — shown on the plan card.</summary>
    public string? ChangeComment { get; init; }
}

public sealed class DiscussionRankingResult
{
    public string OverallSummary { get; init; } = string.Empty;
    public List<string> Risks { get; init; } = [];
    public List<RankedDiscussionItem> Ranked { get; init; } = [];
}

public sealed class RankedDiscussionItem
{
    public string CandidateId { get; init; } = string.Empty;
    public float Score { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<string> Strengths { get; init; } = [];
    public List<string> Weaknesses { get; init; } = [];
}
