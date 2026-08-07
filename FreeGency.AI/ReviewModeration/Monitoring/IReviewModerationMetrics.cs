using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Monitoring;

/// <summary>
/// Thread-safe counters and latency statistics for the review moderation
/// pipeline. Records request counts, cache behaviour, AI calls, outcome actions,
/// success/failure, retries, timeouts, JSON parsing failures, security category
/// hits, and latency percentiles; never holds content, only aggregates. The
/// immutable snapshot is exposed through <see cref="GetSnapshot(double)"/>.
/// </summary>
public interface IReviewModerationMetrics
{
    /// <summary>Records that a review moderation request has started.</summary>
    void RecordRequest();

    /// <summary>Records a cache hit and the time the lookup took.</summary>
    void RecordCacheHit(TimeSpan lookup);

    /// <summary>Records a cache miss and the time the lookup took.</summary>
    void RecordCacheMiss(TimeSpan lookup);

    /// <summary>Records a completed AI call and its latency.</summary>
    void RecordAiCall(TimeSpan latency);

    /// <summary>Records the total request latency (including cache hits and fallbacks).</summary>
    void RecordTotalLatency(TimeSpan latency);

    /// <summary>Records a successfully completed request.</summary>
    void RecordSuccess();

    /// <summary>Records a failed request (AI unavailable, timeout, or cancelled).</summary>
    void RecordFailure();

    /// <summary>Records the final enforcement action of a request.</summary>
    void RecordAction(ReviewAction action);

    /// <summary>Records that one AI retry was performed.</summary>
    void RecordRetry();

    /// <summary>Records that an AI request timed out.</summary>
    void RecordTimeout();

    /// <summary>Records that an AI response could not be parsed as JSON.</summary>
    void RecordJsonParsingFailure();

    /// <summary>Records the current approximate number of live cache entries.</summary>
    void RecordCacheEntries(long count);

    /// <summary>Records a detected security category.</summary>
    void RecordSecurityCategory(string category);

    /// <summary>Records a guardrail signal by stable label (never content).</summary>
    void RecordGuardrailSignal(string signal);

    /// <summary>Marks whether the AI provider is currently available.</summary>
    void MarkAiAvailable(bool available);

    /// <summary>Marks whether the cache is currently available.</summary>
    void MarkCacheAvailable(bool available);

    /// <summary>Gets the total number of requests started.</summary>
    long TotalRequests { get; }

    /// <summary>Gets the number of cache hits.</summary>
    long CacheHits { get; }

    /// <summary>Gets the number of cache misses.</summary>
    long CacheMisses { get; }

    /// <summary>Gets the percentage of requests served from cache.</summary>
    double CacheHitRate { get; }

    /// <summary>Gets the number of AI calls performed.</summary>
    long AiCalls { get; }

    /// <summary>Gets the average AI call latency in milliseconds.</summary>
    double AverageAiLatencyMs { get; }

    /// <summary>Gets the number of AI calls saved by cache hits.</summary>
    long SavedAiCalls { get; }

    /// <summary>Gets the number of successfully completed requests.</summary>
    long SuccessCount { get; }

    /// <summary>Gets the number of failed requests.</summary>
    long FailureCount { get; }

    /// <summary>Gets the percentage of requests that failed.</summary>
    double FailureRate { get; }

    /// <summary>Gets the number of requests whose action was <see cref="ReviewAction.Allow"/>.</summary>
    long Allowed { get; }

    /// <summary>Gets the number of requests whose action was <see cref="ReviewAction.Warn"/>.</summary>
    long Warned { get; }

    /// <summary>Gets the number of requests whose action was <see cref="ReviewAction.Mask"/>.</summary>
    long Masked { get; }

    /// <summary>Gets the number of requests whose action was <see cref="ReviewAction.Reject"/>.</summary>
    long Rejected { get; }

    /// <summary>Gets the number of requests escalated to <see cref="ReviewAction.ManualReview"/>.</summary>
    long ManualReviews { get; }

    /// <summary>Gets the number of AI retries performed.</summary>
    long RetryCount { get; }

    /// <summary>Gets the percentage of AI calls that required a retry.</summary>
    double RetryRate { get; }

    /// <summary>Gets the number of AI timeouts.</summary>
    long TimeoutCount { get; }

    /// <summary>Gets the number of JSON parsing failures.</summary>
    long JsonParsingFailures { get; }

    /// <summary>Gets the approximate current number of live cache entries.</summary>
    long CacheEntries { get; }

    /// <summary>Gets whether the AI provider is currently available.</summary>
    bool AiAvailable { get; }

    /// <summary>Gets whether the cache is currently available.</summary>
    bool CacheAvailable { get; }

    /// <summary>Gets the average total request latency in milliseconds.</summary>
    double AverageTotalLatencyMs { get; }

    /// <summary>
    /// Builds an immutable snapshot. <paramref name="estimatedCostPerAiCallUsd"/>
    /// is used to derive the estimated cost saved by cache hits.
    /// </summary>
    ReviewModerationMetricsSnapshot GetSnapshot(double estimatedCostPerAiCallUsd);

    /// <summary>Resets all counters. Used by tests and operations tooling.</summary>
    void Reset();
}
