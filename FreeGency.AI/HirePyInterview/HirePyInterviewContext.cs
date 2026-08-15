namespace FreeGency.AI.HirePyInterview;

/// <summary>
/// Everything the AI interviewer needs to ground its next reply in: the project brief,
/// the candidate's proposal and the private conversation history so far.
/// </summary>
public sealed class HirePyInterviewContext
{
    public string CandidateName { get; set; } = string.Empty;
    public string ProjectBrief { get; set; } = string.Empty;
    public string ProposalSummary { get; set; } = string.Empty;
    public IReadOnlyList<(string Role, string Content)> History { get; set; } = [];
}
