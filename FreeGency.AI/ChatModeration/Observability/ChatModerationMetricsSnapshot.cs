namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// An immutable snapshot of the moderation metrics at a point in time.
/// Averages are in milliseconds; rates are percentages.
/// </summary>
public sealed record ChatModerationMetricsSnapshot(
    long TotalRequests,
    long TotalAiCalls,
    long TotalCacheHits,
    long TotalCacheMisses,
    double CacheHitRate,
    double CacheMissRate,
    double AverageAiResponseTimeMs,
    double AverageTotalResponseTimeMs,
    double AverageParsingTimeMs,
    long RejectedMessages,
    long WarnedMessages,
    long MaskedMessages,
    long ManualReviews,
    long AllowedMessages,
    long TimeoutCount,
    long JsonParsingFailures,
    long RetryCount,
    double FailureRate,
    double RetryRate,
    string? MostCommonProfanity,
    string? MostCommonCategory,
    string? MostCommonLanguage,
    string? MostCommonScamType,
    string? MostCommonPromptInjection,
    string? MostCommonSensitiveEntity);
