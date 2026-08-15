namespace FreeGency.AI.HirePyInterview.Evaluation;

/// <summary>
/// Structured evaluation of one candidate. <see cref="IsValid"/> is true only when every required
/// score was present and bounded; the caller must not persist an invalid reply.
/// </summary>
public sealed class HirePyEvaluationReply
{
    public bool IsValid { get; set; }

    public int TechnicalScore { get; set; }
    public int RequirementsScore { get; set; }
    public int ArchitectureScore { get; set; }
    public int ImplementationScore { get; set; }
    public int MilestoneScore { get; set; }
    public int TimelineScore { get; set; }
    public int BudgetScore { get; set; }
    public int CommunicationScore { get; set; }
    public int RiskScore { get; set; }
    public int OverallScore { get; set; }

    /// <summary>Client-safe summary points — never raw private conversation content.</summary>
    public IReadOnlyList<string> Strengths { get; set; } = [];

    public IReadOnlyList<string> Concerns { get; set; } = [];

    public IReadOnlyList<string> Risks { get; set; } = [];

    public string? Reason { get; set; }
}
