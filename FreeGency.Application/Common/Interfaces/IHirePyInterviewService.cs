namespace FreeGency.Application.Common.Interfaces;

/// <summary>
/// Drives the private AI interviewer discussion with an accepted freelancer.
/// The conversation itself lives in the existing chat system — this service only
/// manages the HirePyInterview workflow state and the AI turns.
/// </summary>
public interface IHirePyInterviewService
{
    /// <summary>
    /// Creates (once) the private AI chat room and interview row for an accepted freelancer,
    /// then kicks off the AI's opening message.
    /// </summary>
    Task EnsureInterviewAsync(Guid sessionId, Guid proposalId, Guid freelancerUserId, CancellationToken ct = default);

    /// <summary>Runs one AI turn for an interview (opening message or a reply to the latest candidate message).</summary>
    Task ProcessInterviewAsync(Guid interviewId, CancellationToken ct = default);

    /// <summary>Polls all active interviews and processes the ones that are due.</summary>
    Task ProcessPendingAsync(CancellationToken ct = default);
}
