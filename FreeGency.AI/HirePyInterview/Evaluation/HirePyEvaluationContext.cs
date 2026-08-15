namespace FreeGency.AI.HirePyInterview.Evaluation;

/// <summary>Everything the evaluator needs to score ONE candidate and produce the final recommendation input.</summary>
public sealed class HirePyEvaluationContext
{
    public string CandidateName { get; set; } = string.Empty;

    public string ProjectBrief { get; set; } = string.Empty;

    public string ProposalSummary { get; set; } = string.Empty;

    /// <summary>Summary of the candidate's finalized milestone plan (titles, costs, durations).</summary>
    public string MilestonePlanSummary { get; set; } = string.Empty;

    /// <summary>Recent, truncated private discussion transcript (role + content) for the AI only.</summary>
    public string DiscussionSummary { get; set; } = string.Empty;

    /// <summary>Original ranking position (1-based) from the ranking phase — an input, never the decider.</summary>
    public int RankingPosition { get; set; }

    /// <summary>Original ranking score (0-100) when available.</summary>
    public int RankingScore { get; set; }
}
