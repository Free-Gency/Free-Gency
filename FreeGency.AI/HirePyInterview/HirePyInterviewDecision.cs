namespace FreeGency.AI.HirePyInterview;

/// <summary>What the AI wants to do after generating its next reply.</summary>
public enum HirePyInterviewDecision
{
    /// <summary>Continue the technical discussion with a follow-up question.</summary>
    AskQuestion = 0,
    /// <summary>Enough is understood — ask the freelancer for a milestone plan.</summary>
    RequestMilestonePlan = 1
}
