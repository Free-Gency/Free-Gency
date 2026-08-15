namespace FreeGency.AI.HirePyInterview.MilestonePlanning;

/// <summary>What the AI wants to do after reviewing the candidate's milestone plan.</summary>
public enum MilestonePlanningOutcome
{
    /// <summary>The plan needs clarification or revision — keep negotiating in chat.</summary>
    NeedsRevision = 0,
    /// <summary>The plan is acceptable — persist it through the existing milestone-plan flow.</summary>
    Finalize = 1
}
