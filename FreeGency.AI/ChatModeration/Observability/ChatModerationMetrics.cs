using System.Collections.Concurrent;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Thread-safe, in-memory metrics for the AI moderation pipeline. Uses only
/// interlocked/atomic operations and immutable snapshots, so it is safe to share
/// across requests. Tracks request outcomes, AI/cache behaviour, and the most
/// common profanity, category, language, scam type, prompt-injection type, and
/// sensitive entity.
/// </summary>
public sealed class ChatModerationMetrics
{
    private long _totalRequests;
    private long _totalAiCalls;
    private long _totalCacheHits;
    private long _totalCacheMisses;
    private long _aiLatencyTicks;
    private long _totalResponseTicks;
    private long _parsingTicks;
    private long _rejected;
    private long _warned;
    private long _masked;
    private long _manualReviews;
    private long _allowed;
    private long _timeouts;
    private long _jsonParsingFailures;
    private long _retries;
    private long _failures;

    private int _aiAvailable;
    private int _cacheAvailable;

    private readonly ConcurrentDictionary<string, long> _profanityCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _categoryCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _languageCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _scamTypeCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _promptInjectionCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _sensitiveEntityCounts = new(StringComparer.OrdinalIgnoreCase);

    private readonly object _resetSync = new();

    public void RecordRequestStart() => Interlocked.Increment(ref _totalRequests);

    public void RecordCompleted(bool usedCache, TimeSpan totalTime)
    {
        if (usedCache)
            Interlocked.Increment(ref _totalCacheHits);
        else
            Interlocked.Increment(ref _totalCacheMisses);

        Interlocked.Add(ref _totalResponseTicks, totalTime.Ticks);
    }

    public void RecordAiCall(TimeSpan aiLatency)
    {
        Interlocked.Increment(ref _totalAiCalls);
        Interlocked.Add(ref _aiLatencyTicks, aiLatency.Ticks);
    }

    public void RecordParsing(TimeSpan parsingTime)
        => Interlocked.Add(ref _parsingTicks, parsingTime.Ticks);

    public void RecordRetry(int count = 1)
        => Interlocked.Add(ref _retries, count);

    public void RecordTimeout()
    {
        Interlocked.Increment(ref _timeouts);
        Interlocked.Increment(ref _failures);
    }

    public void RecordJsonParsingFailure()
    {
        Interlocked.Increment(ref _jsonParsingFailures);
        Interlocked.Increment(ref _failures);
    }

    public void RecordFailure() => Interlocked.Increment(ref _failures);

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

    public void RecordProfanity(string keyword)
        => _profanityCounts.AddOrUpdate(keyword, 1, (_, value) => value + 1);

    public void RecordCategory(string category)
        => _categoryCounts.AddOrUpdate(category, 1, (_, value) => value + 1);

    public void RecordLanguage(string language)
        => _languageCounts.AddOrUpdate(language, 1, (_, value) => value + 1);

    public void RecordScamType(string type)
        => _scamTypeCounts.AddOrUpdate(type, 1, (_, value) => value + 1);

    public void RecordPromptInjection(string type)
        => _promptInjectionCounts.AddOrUpdate(type, 1, (_, value) => value + 1);

    public void RecordSensitiveEntity(string type)
        => _sensitiveEntityCounts.AddOrUpdate(type, 1, (_, value) => value + 1);

    public void MarkAiAvailable(bool available)
        => Volatile.Write(ref _aiAvailable, available ? 1 : 0);

    public void MarkCacheAvailable(bool available)
        => Volatile.Write(ref _cacheAvailable, available ? 1 : 0);

    public long TotalRequests => Interlocked.Read(ref _totalRequests);
    public long TotalAiCalls => Interlocked.Read(ref _totalAiCalls);
    public long TotalCacheHits => Interlocked.Read(ref _totalCacheHits);
    public long TotalCacheMisses => Interlocked.Read(ref _totalCacheMisses);
    public long RejectedMessages => Interlocked.Read(ref _rejected);
    public long WarnedMessages => Interlocked.Read(ref _warned);
    public long MaskedMessages => Interlocked.Read(ref _masked);
    public long ManualReviews => Interlocked.Read(ref _manualReviews);
    public long AllowedMessages => Interlocked.Read(ref _allowed);
    public long TimeoutCount => Interlocked.Read(ref _timeouts);
    public long JsonParsingFailures => Interlocked.Read(ref _jsonParsingFailures);
    public long RetryCount => Interlocked.Read(ref _retries);
    public long FailureCount => Interlocked.Read(ref _failures);

    public double AverageAiResponseTimeMs => Average(Interlocked.Read(ref _aiLatencyTicks), Interlocked.Read(ref _totalAiCalls));

    public double AverageTotalResponseTimeMs => Average(Interlocked.Read(ref _totalResponseTicks), Interlocked.Read(ref _totalRequests));

    public double AverageParsingTimeMs => Average(Interlocked.Read(ref _parsingTicks), Interlocked.Read(ref _totalAiCalls));

    public double CacheHitRate => Percent(Interlocked.Read(ref _totalCacheHits), Interlocked.Read(ref _totalRequests));

    public double CacheMissRate => Percent(Interlocked.Read(ref _totalCacheMisses), Interlocked.Read(ref _totalRequests));

    public double FailureRate => Percent(Interlocked.Read(ref _failures), Interlocked.Read(ref _totalRequests));

    public double RetryRate => Percent(Interlocked.Read(ref _retries), Interlocked.Read(ref _totalAiCalls));

    public bool AiAvailable => Volatile.Read(ref _aiAvailable) == 1;

    public bool CacheAvailable => Volatile.Read(ref _cacheAvailable) == 1;

    public string? MostCommonProfanity => MostCommon(_profanityCounts);
    public string? MostCommonCategory => MostCommon(_categoryCounts);
    public string? MostCommonLanguage => MostCommon(_languageCounts);
    public string? MostCommonScamType => MostCommon(_scamTypeCounts);
    public string? MostCommonPromptInjection => MostCommon(_promptInjectionCounts);
    public string? MostCommonSensitiveEntity => MostCommon(_sensitiveEntityCounts);

    public ChatModerationMetricsSnapshot GetStatistics()
    {
        return new ChatModerationMetricsSnapshot(
            TotalRequests: Interlocked.Read(ref _totalRequests),
            TotalAiCalls: Interlocked.Read(ref _totalAiCalls),
            TotalCacheHits: Interlocked.Read(ref _totalCacheHits),
            TotalCacheMisses: Interlocked.Read(ref _totalCacheMisses),
            CacheHitRate: CacheHitRate,
            CacheMissRate: CacheMissRate,
            AverageAiResponseTimeMs: AverageAiResponseTimeMs,
            AverageTotalResponseTimeMs: AverageTotalResponseTimeMs,
            AverageParsingTimeMs: AverageParsingTimeMs,
            RejectedMessages: Interlocked.Read(ref _rejected),
            WarnedMessages: Interlocked.Read(ref _warned),
            MaskedMessages: Interlocked.Read(ref _masked),
            ManualReviews: Interlocked.Read(ref _manualReviews),
            AllowedMessages: Interlocked.Read(ref _allowed),
            TimeoutCount: Interlocked.Read(ref _timeouts),
            JsonParsingFailures: Interlocked.Read(ref _jsonParsingFailures),
            RetryCount: Interlocked.Read(ref _retries),
            FailureRate: FailureRate,
            RetryRate: RetryRate,
            MostCommonProfanity: MostCommonProfanity,
            MostCommonCategory: MostCommonCategory,
            MostCommonLanguage: MostCommonLanguage,
            MostCommonScamType: MostCommonScamType,
            MostCommonPromptInjection: MostCommonPromptInjection,
            MostCommonSensitiveEntity: MostCommonSensitiveEntity);
    }

    public void ResetStatistics()
    {
        lock (_resetSync)
        {
            Interlocked.Exchange(ref _totalRequests, 0);
            Interlocked.Exchange(ref _totalAiCalls, 0);
            Interlocked.Exchange(ref _totalCacheHits, 0);
            Interlocked.Exchange(ref _totalCacheMisses, 0);
            Interlocked.Exchange(ref _aiLatencyTicks, 0);
            Interlocked.Exchange(ref _totalResponseTicks, 0);
            Interlocked.Exchange(ref _parsingTicks, 0);
            Interlocked.Exchange(ref _rejected, 0);
            Interlocked.Exchange(ref _warned, 0);
            Interlocked.Exchange(ref _masked, 0);
            Interlocked.Exchange(ref _manualReviews, 0);
            Interlocked.Exchange(ref _allowed, 0);
            Interlocked.Exchange(ref _timeouts, 0);
            Interlocked.Exchange(ref _jsonParsingFailures, 0);
            Interlocked.Exchange(ref _retries, 0);
            Interlocked.Exchange(ref _failures, 0);

            Volatile.Write(ref _aiAvailable, 0);
            Volatile.Write(ref _cacheAvailable, 0);

            _profanityCounts.Clear();
            _categoryCounts.Clear();
            _languageCounts.Clear();
            _scamTypeCounts.Clear();
            _promptInjectionCounts.Clear();
            _sensitiveEntityCounts.Clear();
        }
    }

    public ChatModerationHealth GetCurrentHealth(ChatModerationMonitoringOptions? options = null)
    {
        options ??= new ChatModerationMonitoringOptions();

        var aiAvailable = AiAvailable;
        var cacheAvailable = CacheAvailable;
        var failureRate = FailureRate;
        var retryRate = RetryRate;
        var averageLatency = AverageTotalResponseTimeMs;

        string status;
        if (!aiAvailable || failureRate > options.MaxFailureRate * 100)
        {
            status = "Unhealthy";
        }
        else if (averageLatency > options.AiLatencyWarningSeconds * 1000 ||
                 CacheHitRate < options.MinCacheHitRate * 100)
        {
            status = "Degraded";
        }
        else
        {
            status = "Healthy";
        }

        return new ChatModerationHealth(aiAvailable, cacheAvailable, averageLatency, failureRate, retryRate, status);
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
