using FreeGency.AI.Moderation.Enums;

namespace FreeGency.AI.Moderation.Models;

/// <summary>
/// The outcome of a moderation operation. When the engine completes, <see cref="Success"/>
/// is true and <see cref="Analysis"/> holds the verdict. When the engine fails safely
/// (timeout, circuit breaker open, repeated AI failure), <see cref="Success"/> is false,
/// <see cref="Analysis"/> is null and <see cref="Error"/> carries a client-safe message.
/// </summary>
public sealed record ModerationResult(
    bool Success,
    ModerationAnalysis? Analysis,
    string? Error,
    bool FromCache,
    int RetryCount,
    bool CircuitBroken,
    long ElapsedMs,
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// Gets a value indicating whether the result should be escalated to a human
    /// moderator: either the operation failed safely or the engine itself chose
    /// the <see cref="ModerationAction.ManualReview"/> action.
    /// </summary>
    public bool RequiresManualReview => !Success || Analysis?.Action == ModerationAction.ManualReview;

    /// <summary>Creates a successful result from a completed analysis.</summary>
    public static ModerationResult FromAnalysis(
        ModerationAnalysis analysis,
        long elapsedMs = 0,
        bool fromCache = false,
        int retryCount = 0,
        bool circuitBroken = false)
        => new(true, analysis, null, fromCache, retryCount, circuitBroken, elapsedMs, DateTimeOffset.UtcNow);

    /// <summary>Creates a safe fallback result that requires manual review.</summary>
    public static ModerationResult ManualReview(string error, long elapsedMs = 0, bool circuitBroken = false)
        => new(false, null, error, false, 0, circuitBroken, elapsedMs, DateTimeOffset.UtcNow);
}
