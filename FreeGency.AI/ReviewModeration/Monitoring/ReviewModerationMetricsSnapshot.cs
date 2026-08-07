namespace FreeGency.AI.ReviewModeration.Monitoring;

/// <summary>
/// An immutable snapshot of the review moderation telemetry. Rates are
/// percentages; latencies are in milliseconds. <see cref="SavedAiCalls"/> equals
/// the cache hit count (every hit avoids one AI call); the estimated cost saved
/// is derived from it using the configured cost per call.
/// </summary>
public sealed record ReviewModerationMetricsSnapshot(
    long TotalRequests,
    long CacheHits,
    long CacheMisses,
    double CacheHitRate,
    long AiCalls,
    double AverageAiLatencyMs,
    double AverageCacheLookupMs,
    long SavedAiCalls,
    double EstimatedCostSavedUsd,
    long SuccessCount,
    long FailureCount,
    double FailureRate,
    long Allowed,
    long Warned,
    long Masked,
    long Rejected,
    long ManualReviews,
    long RetryCount,
    double RetryRate,
    long TimeoutCount,
    long JsonParsingFailures,
    double AiLatencyMinMs,
    double AiLatencyMaxMs,
    double AiLatencyP50Ms,
    double AiLatencyP95Ms,
    double AiLatencyP99Ms,
    double TotalLatencyMinMs,
    double TotalLatencyMaxMs,
    double AverageTotalLatencyMs,
    double TotalLatencyP50Ms,
    double TotalLatencyP95Ms,
    double TotalLatencyP99Ms,
    long CacheEntries,
    bool AiAvailable,
    bool CacheAvailable,
    IReadOnlyDictionary<string, long> SecurityCategoryCounts,
    IReadOnlyDictionary<string, long> GuardrailSignalCounts);
