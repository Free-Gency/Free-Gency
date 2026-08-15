namespace FreeGency.AI.HirePyInterview;

/// <summary>
/// Produces the next dynamic AI interviewer message for a private freelancer discussion.
/// Uses the existing <c>IChatCompletionService</c>; conversation history is flattened into
/// the prompt because the underlying gateway currently only forwards the last user message.
/// </summary>
public interface IHirePyInterviewAgent
{
    Task<HirePyInterviewReply> GetNextReplyAsync(HirePyInterviewContext context, CancellationToken ct = default);
}
