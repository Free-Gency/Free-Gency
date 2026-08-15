namespace FreeGency.Domain.Enums;

public enum HirePyInterviewStatus
{
    /// <summary>Interview is active — the AI drives the conversation until a milestone plan is requested.</summary>
    Started = 0,
    /// <summary>The AI concluded the discussion and asked the candidate for a milestone plan.</summary>
    MilestonePlanRequested = 1,
    /// <summary>A candidate milestone-plan message arrived and the AI is reviewing it.</summary>
    MilestonePlanReceived = 2,
    /// <summary>The AI asked the candidate to revise the milestone plan (negotiation round).</summary>
    MilestoneRevisionRequested = 3,
    /// <summary>The AI accepted the plan; it was persisted through the existing milestone-plan flow.</summary>
    MilestonePlanFinalized = 4,
    /// <summary>The interview could not proceed (e.g. repeated AI failures).</summary>
    Failed = 5
}
