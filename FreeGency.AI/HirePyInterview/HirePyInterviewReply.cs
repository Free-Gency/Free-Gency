namespace FreeGency.AI.HirePyInterview;

/// <summary>One generated AI interviewer reply.</summary>
public sealed class HirePyInterviewReply
{
    public string Message { get; set; } = string.Empty;
    public HirePyInterviewDecision Decision { get; set; } = HirePyInterviewDecision.AskQuestion;
}
