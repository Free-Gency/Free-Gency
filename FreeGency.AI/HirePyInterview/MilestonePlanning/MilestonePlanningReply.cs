namespace FreeGency.AI.HirePyInterview.MilestonePlanning;

/// <summary>Structured reply the planner produces for one negotiation round.</summary>
public sealed class MilestonePlanningReply
{
    /// <summary>Message posted to the candidate (question when revising, confirmation when finalizing).</summary>
    public string Message { get; set; } = string.Empty;

    public MilestonePlanningOutcome Outcome { get; set; }

    /// <summary>Concise review points (missing scope, unrealistic cost/duration, missing criteria, ...).</summary>
    public IReadOnlyList<string> Issues { get; set; } = [];

    /// <summary>Only populated when <see cref="Outcome"/> is Finalize. Validated and mapped.</summary>
    public IReadOnlyList<ProposedMilestone> Milestones { get; set; } = [];
}
