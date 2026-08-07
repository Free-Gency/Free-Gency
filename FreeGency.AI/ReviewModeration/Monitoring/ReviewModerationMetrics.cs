using System.Collections.Concurrent;
using System.Threading;
using FreeGency.AI.Monitoring;
using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Monitoring;

/// <summary>
/// Default, thread-safe in-memory implementation of <see cref="IReviewModerationMetrics"/>.
/// Uses only interlocked operations, volatile flags, and lock-guarded latency
/// stats, so it is safe to share across concurrent requests. Never stores content;
/// only counters, aggregates, and latency percentiles.
/// </summary>
public sealed class ReviewModerationMetrics : IReviewModerationMetrics
{
    private long _requests;
    private long _cacheHits;
    private long _cacheMisses;
    private long _cacheLookupTicks;
    private long _aiCalls;
    private long _aiLatencyTicks;
    private long _success;
    private long _failures;
    private long _allowed;
    private long _warned;
    private long _masked;
    private long _rejected;
    private long _manualReviews;
    private long _retries;
    private long _timeouts;
    private long _jsonParsingFailures;
    private long _cacheEntries;

    private int _aiAvailable;
    private int _cacheAvailable;

    private readonly ModerationLatencyStats _aiLatencyStats = new();
    private readonly ModerationLatencyStats _totalLatencyStats = new();
    private readonly ConcurrentDictionary<string, long> _securityCategoryCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _guardrailSignalCounts = new(StringComparer.OrdinalIgnoreCase);

    private readonly object _resetSync = new();

    public void RecordRequest() => Interlocked.Increment(ref _requests);

    public void RecordCacheHit(TimeSpan lookup)
    {
        Interlocked.Increment(ref _cacheHits);
        Interlocked.Add(ref _cacheLookupTicks, lookup.Ticks);
    }

    public void RecordCacheMiss(TimeSpan lookup)
    {
        Interlocked.Increment(ref _cacheMisses);
        Interlocked.Add(ref _cacheLookupTicks, lookup.Ticks);
    }

    public void RecordAiCall(TimeSpan latency)
    {
        _aiLatencyStats.Add(latency);
        Interlocked.Increment(ref _aiCalls);
        Interlocked.Add(ref _aiLatencyTicks, latency.Ticks);
    }

    public void RecordTotalLatency(TimeSpan latency) => _totalLatencyStats.Add(latency);

    public void RecordSuccess() => Interlocked.Increment(ref _success);

    public void RecordFailure() => Interlocked.Increment(ref _failures);

    public void RecordAction(ReviewAction action)
    {
        switch (action)
        {
            case ReviewAction.Allow:
                Interlocked.Increment(ref _allowed);
                break;
            case ReviewAction.Warn:
                Interlocked.Increment(ref _warned);
                break;
            case ReviewAction.Mask:
                Interlocked.Increment(ref _masked);
                break;
            case ReviewAction.Reject:
                Interlocked.Increment(ref _rejected);
                break;
            case ReviewAction.ManualReview:
                Interlocked.Increment(ref _manualReviews);
                break;
        }
    }

    public void RecordRetry() => Interlocked.Increment(ref _retries);

    public void RecordTimeout() => Interlocked.Increment(ref _timeouts);

    public void RecordJsonParsingFailure() => Interlocked.Increment(ref _jsonParsingFailures);

    public void RecordCacheEntries(long count) => Interlocked.Exchange(ref _cacheEntries, count);

    public void RecordSecurityCategory(string category)
        => _securityCategoryCounts.AddOrUpdate(category, 1, (_, value) => value + 1);

    public void RecordGuardrailSignal(string signal)
        => _guardrailSignalCounts.AddOrUpdate(signal, 1, (_, value) => value + 1);

    public void MarkAiAvailable(bool available)
        => Volatile.Write(ref _aiAvailable, available ? 1 : 0);

    public void MarkCacheAvailable(bool available)
        => Volatile.Write(ref _cacheAvailable, available ? 1 : 0);

    public long TotalRequests => Interlocked.Read(ref _requests);
    public long CacheHits => Interlocked.Read(ref _cacheHits);
    public long CacheMisses => Interlocked.Read(ref _cacheMisses);
    public long SavedAiCalls => CacheHits;
    public long AiCalls => Interlocked.Read(ref _aiCalls);

    public double CacheHitRate => Percent(CacheHits, TotalRequests);

    public double AverageCacheLookupMs => Average(
        Interlocked.Read(ref _cacheLookupTicks),
        CacheHits + CacheMisses);

    public double AverageAiLatencyMs => Average(
        Interlocked.Read(ref _aiLatencyTicks),
        AiCalls);

    public long SuccessCount => Interlocked.Read(ref _success);
    public long FailureCount => Interlocked.Read(ref _failures);
    public double FailureRate => Percent(FailureCount, TotalRequests);

    public long Allowed => Interlocked.Read(ref _allowed);
    public long Warned => Interlocked.Read(ref _warned);
    public long Masked => Interlocked.Read(ref _masked);
    public long Rejected => Interlocked.Read(ref _rejected);
    public long ManualReviews => Interlocked.Read(ref _manualReviews);

    public long RetryCount => Interlocked.Read(ref _retries);
    public double RetryRate => Percent(RetryCount, AiCalls);

    public long TimeoutCount => Interlocked.Read(ref _timeouts);
    public long JsonParsingFailures => Interlocked.Read(ref _jsonParsingFailures);
    public long CacheEntries => Interlocked.Read(ref _cacheEntries);

    public bool AiAvailable => Volatile.Read(ref _aiAvailable) == 1;
    public bool CacheAvailable => Volatile.Read(ref _cacheAvailable) == 1;

    public double AverageTotalLatencyMs => _totalLatencyStats.GetSummary().AverageMs;

    public IReadOnlyDictionary<string, long> SecurityCategoryCounts => _securityCategoryCounts;

    public IReadOnlyDictionary<string, long> GuardrailSignalCounts => _guardrailSignalCounts;

    public ReviewModerationMetricsSnapshot GetSnapshot(double estimatedCostPerAiCallUsd)
    {
        var ai = _aiLatencyStats.GetSummary();
        var total = _totalLatencyStats.GetSummary();

        return new ReviewModerationMetricsSnapshot(
            TotalRequests,
            CacheHits,
            CacheMisses,
            CacheHitRate,
            AiCalls,
            AverageAiLatencyMs,
            AverageCacheLookupMs,
            SavedAiCalls,
            Math.Round(SavedAiCalls * estimatedCostPerAiCallUsd, 6),
            SuccessCount,
            FailureCount,
            FailureRate,
            Allowed,
            Warned,
            Masked,
            Rejected,
            ManualReviews,
            RetryCount,
            RetryRate,
            TimeoutCount,
            JsonParsingFailures,
            ai.MinMs,
            ai.MaxMs,
            ai.P50Ms,
            ai.P95Ms,
            ai.P99Ms,
            total.MinMs,
            total.MaxMs,
            total.AverageMs,
            total.P50Ms,
            total.P95Ms,
            total.P99Ms,
            CacheEntries,
            AiAvailable,
            CacheAvailable,
            SecurityCategoryCounts,
            GuardrailSignalCounts);
    }

    public void Reset()
    {
        lock (_resetSync)
        {
            Interlocked.Exchange(ref _requests, 0);
            Interlocked.Exchange(ref _cacheHits, 0);
            Interlocked.Exchange(ref _cacheMisses, 0);
            Interlocked.Exchange(ref _cacheLookupTicks, 0);
            Interlocked.Exchange(ref _aiCalls, 0);
            Interlocked.Exchange(ref _aiLatencyTicks, 0);
            Interlocked.Exchange(ref _success, 0);
            Interlocked.Exchange(ref _failures, 0);
            Interlocked.Exchange(ref _allowed, 0);
            Interlocked.Exchange(ref _warned, 0);
            Interlocked.Exchange(ref _masked, 0);
            Interlocked.Exchange(ref _rejected, 0);
            Interlocked.Exchange(ref _manualReviews, 0);
            Interlocked.Exchange(ref _retries, 0);
            Interlocked.Exchange(ref _timeouts, 0);
            Interlocked.Exchange(ref _jsonParsingFailures, 0);
            Interlocked.Exchange(ref _cacheEntries, 0);

            Volatile.Write(ref _aiAvailable, 0);
            Volatile.Write(ref _cacheAvailable, 0);

            _aiLatencyStats.Reset();
            _totalLatencyStats.Reset();
            _securityCategoryCounts.Clear();
            _guardrailSignalCounts.Clear();
        }
    }

    private static double Average(long ticks, long count)
        => count == 0 ? 0 : ticks / (double)TimeSpan.TicksPerMillisecond / count;

    private static double Percent(long numerator, long denominator)
        => denominator == 0 ? 0 : numerator * 100.0 / denominator;
}
