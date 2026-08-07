namespace FreeGency.AI.Moderation.Observability;

/// <summary>
/// An immutable snapshot of the moderation metrics at a point in time.
/// Rates are percentages; latencies are in milliseconds.
/// </summary>
public sealed record ModerationMetricsSnapshot(
    long TotalRequests,
    long TotalAiCalls,
    long TotalCacheHits,
    long TotalCacheMisses,
    double CacheHitRate,
    double AverageAiLatencyMs,
    long Timeouts,
    long Retries,
    long CircuitBreakerTrips,
    long Failures,
    double FailureRate,
    long Allowed,
    long Warned,
    long Masked,
    long Rejected,
    long ManualReviews,
    string? MostCommonCategory);
