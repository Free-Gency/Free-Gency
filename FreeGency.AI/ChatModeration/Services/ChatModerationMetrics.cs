using System.Collections.Concurrent;
using FreeGency.AI.ChatModeration.Interfaces;

namespace FreeGency.AI.ChatModeration.Services;

public sealed class ChatModerationMetrics : IChatModerationMetrics
{
    private long _totalRequests;
    private long _totalCacheHits;
    private long _totalElapsedMilliseconds;
    private readonly ConcurrentDictionary<string, long> _decisions = new();

    public void RecordRequest(string decision, double confidence, TimeSpan elapsed, bool fromCache)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Add(ref _totalElapsedMilliseconds, (long)elapsed.TotalMilliseconds);

        if (fromCache)
            Interlocked.Increment(ref _totalCacheHits);

        _decisions.AddOrUpdate(decision, 1, (_, count) => count + 1);
    }

    public IReadOnlyDictionary<string, long> Snapshot()
    {
        var snapshot = new Dictionary<string, long>
        {
            ["TotalRequests"] = Interlocked.Read(ref _totalRequests),
            ["CacheHits"] = Interlocked.Read(ref _totalCacheHits),
            ["TotalElapsedMilliseconds"] = Interlocked.Read(ref _totalElapsedMilliseconds)
        };

        foreach (var (decision, count) in _decisions)
            snapshot[decision] = count;

        return snapshot;
    }
}
