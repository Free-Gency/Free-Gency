using System.Collections.Concurrent;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Thread-safe, in-memory metrics for the chat moderation engine.
/// Uses only immutable state and interlocked/atomic operations.
/// </summary>
public sealed class ChatSecurityMetrics
{
    private long _total;
    private long _cacheHits;
    private long _rejected;
    private long _masked;
    private long _warned;
    private long _safe;
    private long _aiLatencyTicks;
    private long _processingTicks;

    private readonly ConcurrentDictionary<string, long> _categoryCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _profanityCounts = new(StringComparer.OrdinalIgnoreCase);

    public void RecordRequest(ModerationAction action, bool usedCache, TimeSpan processingTime, TimeSpan aiLatency)
    {
        Interlocked.Increment(ref _total);

        if (usedCache)
            Interlocked.Increment(ref _cacheHits);

        switch (action)
        {
            case ModerationAction.Reject:
                Interlocked.Increment(ref _rejected);
                break;
            case ModerationAction.Mask:
                Interlocked.Increment(ref _masked);
                break;
            case ModerationAction.Warn:
                Interlocked.Increment(ref _warned);
                break;
            case ModerationAction.Allow:
                Interlocked.Increment(ref _safe);
                break;
        }

        Interlocked.Add(ref _aiLatencyTicks, aiLatency.Ticks);
        Interlocked.Add(ref _processingTicks, processingTime.Ticks);
    }

    public void RecordCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return;

        _categoryCounts.AddOrUpdate(category, 1, (_, value) => value + 1);
    }

    public void RecordProfanity(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return;

        _profanityCounts.AddOrUpdate(keyword, 1, (_, value) => value + 1);
    }

    public long TotalCount => Interlocked.Read(ref _total);

    public double AverageAiLatencyMs => PercentageOf(Interlocked.Read(ref _aiLatencyTicks), TimeSpan.TicksPerMillisecond);

    public double AverageProcessingTimeMs => PercentageOf(Interlocked.Read(ref _processingTicks), TimeSpan.TicksPerMillisecond);

    public double RejectedPercent => Percent(_rejected);

    public double MaskedPercent => Percent(_masked);

    public double WarnPercent => Percent(_warned);

    public double SafePercent => Percent(_safe);

    public double CacheHitPercent => Percent(_cacheHits);

    public string? MostDetectedCategory =>
        _categoryCounts.Count == 0 ? null : _categoryCounts.OrderByDescending(kv => kv.Value).First().Key;

    public string? MostDetectedProfanity =>
        _profanityCounts.Count == 0 ? null : _profanityCounts.OrderByDescending(kv => kv.Value).First().Key;

    private double Percent(long count)
    {
        var total = Interlocked.Read(ref _total);
        return total == 0 ? 0 : count * 100.0 / total;
    }

    private double PercentageOf(long ticks, long divisor)
    {
        var total = Interlocked.Read(ref _total);
        return total == 0 ? 0 : ticks / (double)divisor / total;
    }
}
