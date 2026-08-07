using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Guardrails;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Validators;

namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Structured logger for the review moderation pipeline. Every method stamps its
/// output with the audit fields from the current correlation context: UTC
/// timestamp, request id, and correlation id. It never logs raw review text;
/// reviews are represented by hashes and lengths only. The interface is content
/// agnostic so the same pattern can be reused for comment, portfolio, profile,
/// and project moderation without changing the implementation.
/// </summary>
public interface IReviewModerationLogger
{
    /// <summary>Writes the Review Moderation Started event.</summary>
    void LogModerationStarted(ReviewModerationRequestContext context);

    /// <summary>Writes the Review Moderation Completed event.</summary>
    void LogModerationCompleted(ReviewModerationRequestContext context, ReviewModerationResponse response, TimeSpan totalTime);

    /// <summary>Writes the Cache Hit event.</summary>
    void LogCacheHit();

    /// <summary>Writes the Cache Miss event.</summary>
    void LogCacheMiss();

    /// <summary>Writes the AI Request Started event.</summary>
    void LogAiStarted();

    /// <summary>Writes the AI Request Completed event.</summary>
    void LogAiFinished(TimeSpan elapsed);

    /// <summary>Writes the AI Request Failed event.</summary>
    void LogAiFailed(string? reason, TimeSpan elapsed);

    /// <summary>Writes the Retry Started event.</summary>
    void LogRetryStarted(int attempt, int maxRetries);

    /// <summary>Writes the Retry Failed event.</summary>
    void LogRetryFailed(int attempt, int maxRetries);

    /// <summary>Writes the Timeout event.</summary>
    void LogTimeout(TimeSpan elapsed);

    /// <summary>Writes the Manual Review Triggered event.</summary>
    void LogManualReview(ReviewModerationRequestContext context, ReviewModerationResponse response);

    /// <summary>Writes an audit record for the completed request.</summary>
    void LogAudit(ReviewModerationRequestContext context, ReviewModerationResponse response, TimeSpan processingTime);

    /// <summary>Writes an Error for an unexpected exception.</summary>
    void LogUnexpectedException(Exception exception);

    /// <summary>Writes the guardrail findings for a request (metadata only, never content).</summary>
    void LogGuardrails(ReviewGuardrailOutcome outcome);

    /// <summary>Writes the AI response validation rejection (metadata only, never content).</summary>
    void LogResponseValidationRejected(ReviewResponseValidationResult validation);

    /// <summary>Evaluates performance thresholds and writes alerts (rate-limited).</summary>
    void LogPerformanceAlerts(IReviewModerationMetrics metrics, TimeSpan aiLatency);
}
