namespace FreeGency.AI.HirePyInterview.MilestonePlanning;

/// <summary>Everything the planner needs to review one round of the candidate's milestone plan.</summary>
public sealed class MilestonePlanningContext
{
    public string CandidateName { get; set; } = string.Empty;

    public string ProjectBrief { get; set; } = string.Empty;

    public string ProposalSummary { get; set; } = string.Empty;

    /// <summary>Recent chat turns (role + content) so the AI stays grounded in the discussion.</summary>
    public IReadOnlyList<MilestonePlanningHistoryEntry> History { get; set; } = [];

    /// <summary>True when this is the last allowed negotiation round — the AI must finalize.</summary>
    public bool IsFinalAttempt { get; set; }
}

public sealed class MilestonePlanningHistoryEntry
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
