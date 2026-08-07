namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// An immutable snapshot of the cache state, safe to expose to diagnostics
/// endpoints or health checks. All values are read atomically.
/// </summary>
public sealed record ChatModerationCacheStatistics(
    int CurrentEntries,
    long TotalRequests,
    long TotalHits,
    long TotalMisses,
    double HitRate,
    double AverageRetrievalTimeMs,
    double AverageInsertTimeMs,
    double AverageComputeTimeMs,
    long TotalEvictions,
    long ExpiredEvictions,
    long CapacityEvictions);
