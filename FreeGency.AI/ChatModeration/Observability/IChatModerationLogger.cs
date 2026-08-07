using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Structured logger for the AI moderation pipeline. Every method stamps its
/// output with the audit fields from <see cref="ChatModerationRequestContext"/>:
/// UTC timestamp, correlation id, and conversation id. The interface is content
/// agnostic so it can be reused for proposal ranking, review, comment, project
/// description, portfolio, and profile moderation without changing the
/// implementation.
/// </summary>
public interface IChatModerationLogger
{
    /// <summary>Creates a request context with a fresh request id and correlation id.</summary>
    ChatModerationRequestContext BeginRequest(
        ChatModerationRequest request,
        string messageHash,
        ContentLanguage language,
        string promptVersion);

    /// <summary>Writes an audit record for the completed request.</summary>
    void LogAudit(ChatModerationRequestContext context, ChatModerationResponse response, TimeSpan processingTime, int retryCount);

    /// <summary>Writes an Information audit trail for the given event.</summary>
    void LogRequestStarted(ChatModerationRequestContext context);

    void LogCacheHit(ChatModerationRequestContext context, TimeSpan lookupTime);

    void LogCacheMiss(ChatModerationRequestContext context);

    void LogAiStarted(ChatModerationRequestContext context);

    void LogAiFinished(ChatModerationRequestContext context, TimeSpan elapsed, bool usedCache);

    void LogAllowed(ChatModerationRequestContext context, ChatModerationResponse response);

    /// <summary>Writes a Warning for messages that were warned or flagged high risk.</summary>
    void LogWarned(ChatModerationRequestContext context, ChatModerationResponse response);

    void LogHighRisk(ChatModerationRequestContext context, ChatModerationResponse response);

    void LogRejected(ChatModerationRequestContext context, ChatModerationResponse response);

    void LogMasked(ChatModerationRequestContext context, ChatModerationResponse response);

    void LogManualReview(ChatModerationRequestContext context, ChatModerationResponse response);

    /// <summary>Writes an Error for a timed-out moderation call.</summary>
    void LogTimeout(ChatModerationRequestContext context, TimeSpan elapsed);

    /// <summary>Writes an Error for a network failure during the AI call.</summary>
    void LogNetworkFailure(ChatModerationRequestContext context, TimeSpan elapsed);

    /// <summary>Writes an Error when the AI call failed.</summary>
    void LogAiFailure(ChatModerationRequestContext context, string? reason, TimeSpan elapsed);

    /// <summary>Writes an Error when the AI response could not be parsed as JSON.</summary>
    void LogInvalidJson(ChatModerationRequestContext context);

    /// <summary>Writes a Warning when the request was cancelled.</summary>
    void LogCancelled(ChatModerationRequestContext context);

    /// <summary>Writes an Error for an unexpected exception.</summary>
    void LogUnexpectedException(ChatModerationRequestContext context, Exception exception);

    /// <summary>Evaluates performance thresholds and writes alerts (rate-limited).</summary>
    void LogPerformanceAlerts(ChatModerationMetrics metrics, TimeSpan aiLatency);
}
