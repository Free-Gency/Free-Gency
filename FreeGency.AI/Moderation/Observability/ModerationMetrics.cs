using System.Collections.Concurrent;
using FreeGency.AI.Moderation.Enums;

namespace FreeGency.AI.Moderation.Observability;

/// <summary>
/// Thread-safe, in-memory metrics for the moderation core. Uses only
/// interlocked/atomic operations and immutable snapshots, so it is safe to share
/// across requests. Never holds content; only counts, hashes, and latencies.
/// </summary>
public sealed class ModerationMetrics
{
    private long _totalRequests;
    private long _totalAiCalls;
    private long _totalCacheHits;
    private long _totalCacheMisses;
    private long _aiLatencyTicks;
    private long _failures;
    private long _timeouts;
    private long _retries;
    private long _circuitBreakerTrips;
    private long _allowed;
    private long _warned;
    private long _masked;
    private long _rejected;
    private long _manualReviews;

    private readonly ConcurrentDictionary<string, long> _categoryCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _resetSync = new();

    /// <summary>Records that a moderation request has started.</summary>
    public void RecordRequestStarted() => Interlocked.Increment(ref _totalRequests);

    /// <summary>Records a cache hit.</summary>
    public void RecordCacheHit() => Interlocked.Increment(ref _totalCacheHits);

    /// <summary>Records a cache miss.</summary>
    public void RecordCacheMiss() => Interlocked.Increment(ref _totalCacheMisses);

    /// <summary>Records a completed AI call and its latency.</summary>
    public void RecordAiCall(TimeSpan latency)
    {
        Interlocked.Increment(ref _totalAiCalls);
        Interlocked.Add(ref _aiLatencyTicks, latency.Ticks);
    }

    /// <summary>Records a timed-out AI call.</summary>
    public void RecordTimeout()
    {
        Interlocked.Increment(ref _timeouts);
        Interlocked.Increment(ref _failures);
    }

    /// <summary>Records a retry.</summary>
    public void RecordRetry(int count = 1) => Interlocked.Add(ref _retries, count);

    /// <summary>Records that the circuit breaker short-circuited a request.</summary>
    public void RecordCircuitTrip() => Interlocked.Increment(ref _circuitBreakerTrips);

    /// <summary>Records a failed request.</summary>
    public void RecordFailure() => Interlocked.Increment(ref _failures);

    /// <summary>Records the recommended action of a completed analysis.</summary>
    public void RecordAction(ModerationAction action)
    {
        switch (action)
        {
            case ModerationAction.Allow:
                Interlocked.Increment(ref _allowed);
                break;
            case ModerationAction.Warn:
                Interlocked.Increment(ref _warned);
                break;
            case ModerationAction.Mask:
                Interlocked.Increment(ref _masked);
                break;
            case ModerationAction.Reject:
                Interlocked.Increment(ref _rejected);
                break;
            case ModerationAction.ManualReview:
                Interlocked.Increment(ref _manualReviews);
                break;
        }
    }

    /// <summary>Records a detected violation category.</summary>
    public void RecordCategory(string category)
        => _categoryCounts.AddOrUpdate(category, 1, (_, value) => value + 1);

    public long TotalRequests => Interlocked.Read(ref _totalRequests);
    public long TotalAiCalls => Interlocked.Read(ref _totalAiCalls);
    public long TotalCacheHits => Interlocked.Read(ref _totalCacheHits);
    public long TotalCacheMisses => Interlocked.Read(ref _totalCacheMisses);
    public long Failures => Interlocked.Read(ref _failures);
    public long Timeouts => Interlocked.Read(ref _timeouts);
    public long Retries => Interlocked.Read(ref _retries);
    public long CircuitBreakerTrips => Interlocked.Read(ref _circuitBreakerTrips);
    public long Allowed => Interlocked.Read(ref _allowed);
    public long Warned => Interlocked.Read(ref _warned);
    public long Masked => Interlocked.Read(ref _masked);
    public long Rejected => Interlocked.Read(ref _rejected);
    public long ManualReviews => Interlocked.Read(ref _manualReviews);

    /// <summary>Gets the cache hit rate as a percentage (0..100).</summary>
    public double CacheHitRate => Percent(TotalCacheHits, TotalRequests);

    /// <summary>Gets the failure rate as a percentage (0..100).</summary>
    public double FailureRate => Percent(Failures, TotalRequests);

    /// <summary>Gets the average AI call latency in milliseconds.</summary>
    public double AverageAiLatencyMs => Average(Interlocked.Read(ref _aiLatencyTicks), TotalAiCalls);

    /// <summary>Gets the most frequently detected category, or null when none.</summary>
    public string? MostCommonCategory => MostCommon(_categoryCounts);

    /// <summary>Builds an immutable snapshot of the current metrics.</summary>
    public ModerationMetricsSnapshot GetSnapshot()
    {
        return new ModerationMetricsSnapshot(
            TotalRequests,
            TotalAiCalls,
            TotalCacheHits,
            TotalCacheMisses,
            CacheHitRate,
            AverageAiLatencyMs,
            Timeouts,
            Retries,
            CircuitBreakerTrips,
            Failures,
            FailureRate,
            Allowed,
            Warned,
            Masked,
            Rejected,
            ManualReviews,
            MostCommonCategory);
    }

    /// <summary>Resets all counters. Used by tests and operations tooling.</summary>
    public void Reset()
    {
        lock (_resetSync)
        {
            Interlocked.Exchange(ref _totalRequests, 0);
            Interlocked.Exchange(ref _totalAiCalls, 0);
            Interlocked.Exchange(ref _totalCacheHits, 0);
            Interlocked.Exchange(ref _totalCacheMisses, 0);
            Interlocked.Exchange(ref _aiLatencyTicks, 0);
            Interlocked.Exchange(ref _failures, 0);
            Interlocked.Exchange(ref _timeouts, 0);
            Interlocked.Exchange(ref _retries, 0);
            Interlocked.Exchange(ref _circuitBreakerTrips, 0);
            Interlocked.Exchange(ref _allowed, 0);
            Interlocked.Exchange(ref _warned, 0);
            Interlocked.Exchange(ref _masked, 0);
            Interlocked.Exchange(ref _rejected, 0);
            Interlocked.Exchange(ref _manualReviews, 0);
            _categoryCounts.Clear();
        }
    }

    private static string? MostCommon(ConcurrentDictionary<string, long> counts)
    {
        if (counts.Count == 0)
            return null;

        return counts.OrderByDescending(pair => pair.Value).First().Key;
    }

    private static double Average(long ticks, long count)
    {
        return count == 0 ? 0 : ticks / (double)TimeSpan.TicksPerMillisecond / count;
    }

    private static double Percent(long numerator, long denominator)
    {
        return denominator == 0 ? 0 : numerator * 100.0 / denominator;
    }
}
