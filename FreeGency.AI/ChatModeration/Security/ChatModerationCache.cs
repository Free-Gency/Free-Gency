using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Thread-safe, size-bounded moderation cache built on <see cref="IMemoryCache"/>.
///
/// Guarantees:
/// - Cache-first: reads hit memory; only misses run the factory.
/// - Single-flight: concurrent callers for the same key share one in-flight
///   computation via <see cref="Lazy{T}"/>, so the AI is never called twice for
///   the same key.
/// - Bounded: the cache is created with a size limit, so the least recently used
///   entries are evicted first (LRU) and memory stays bounded.
/// - Selective: a factory may mark its value non-cacheable (manual review,
///   timeouts, unparseable AI responses) and it will be returned but not stored.
/// </summary>
public sealed class ChatModerationCache : IChatModerationCache
{
    private const int EntrySize = 1;

    private readonly IMemoryCache _cache;
    private readonly ChatModerationCacheOptions _options;
    private readonly ILogger<ChatModerationCache> _logger;

    private readonly ConcurrentDictionary<string, Lazy<Task<object?>>> _inFlight = new(StringComparer.Ordinal);

    private long _currentEntries;
    private long _totalRequests;
    private long _hits;
    private long _misses;
    private long _retrievalTicks;
    private long _insertCount;
    private long _insertTicks;
    private long _computeCount;
    private long _computeTicks;
    private long _evictions;
    private long _expired;
    private long _capacityEvictions;

    public ChatModerationCache(
        IMemoryCache cache,
        IOptions<ChatModerationCacheOptions> options,
        ILogger<ChatModerationCache> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        var stopwatch = Stopwatch.StartNew();
        var hit = false;

        try
        {
            if (_cache.TryGetValue(key, out var raw) && raw is T typed)
            {
                Interlocked.Increment(ref _hits);
                value = typed;
                hit = true;
                return true;
            }

            Interlocked.Increment(ref _misses);
            value = default;
            return false;
        }
        finally
        {
            stopwatch.Stop();
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Add(ref _retrievalTicks, stopwatch.Elapsed.Ticks);
            LogRetrieval(key, stopwatch.Elapsed, hit);
        }
    }

    public void Set<T>(string key, T value)
    {
        StoreValue(key, value);
    }

    public void Remove(string key)
    {
        _inFlight.TryRemove(key, out _);
        _cache.Remove(key);
    }

    public async Task<CacheResult<T>> GetOrAddAsync<T>(
        string key,
        Func<CancellationToken, Task<CacheResult<T>>> factory,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var retrieval = Stopwatch.StartNew();

        if (_cache.TryGetValue(key, out var raw))
        {
            if (raw is T hit)
            {
                retrieval.Stop();
                Interlocked.Add(ref _retrievalTicks, retrieval.Elapsed.Ticks);
                Interlocked.Increment(ref _totalRequests);
                Interlocked.Increment(ref _hits);
                LogRetrieval(key, retrieval.Elapsed, cacheHit: true);
                return new CacheResult<T>(hit, Cacheable: true, FromCache: true);
            }

            // A value of a different type exists under the same key. Treat it as
            // a miss and let the factory overwrite it.
            _logger.LogDebug("Cache key collision on type. Key={Key} ExistingType={ExistingType} RequestedType={RequestedType}",
                key, raw.GetType().Name, typeof(T).Name);
        }

        retrieval.Stop();
        Interlocked.Add(ref _retrievalTicks, retrieval.Elapsed.Ticks);
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _misses);
        LogRetrieval(key, retrieval.Elapsed, cacheHit: false);

        var lazy = _inFlight.GetOrAdd(key, _ => new Lazy<Task<object?>>(() => ExecuteAndStoreAsync(key, factory, ct)));

        try
        {
            var value = await lazy.Value.WaitAsync(ct);
            if (value is CacheResult<T> result)
                return result;

            throw new InvalidOperationException($"The factory for key '{key}' produced an unexpected type.");
        }
        finally
        {
            _inFlight.TryRemove(key, out _);
        }
    }

    public ChatModerationCacheStatistics GetStatistics()
    {
        var total = Interlocked.Read(ref _totalRequests);
        var hits = Interlocked.Read(ref _hits);

        return new ChatModerationCacheStatistics(
            CurrentEntries: (int)Interlocked.Read(ref _currentEntries),
            TotalRequests: total,
            TotalHits: hits,
            TotalMisses: Interlocked.Read(ref _misses),
            HitRate: total == 0 ? 0 : hits * 100.0 / total,
            AverageRetrievalTimeMs: AverageTicks(Interlocked.Read(ref _retrievalTicks), total),
            AverageInsertTimeMs: AverageTicks(Interlocked.Read(ref _insertTicks), Interlocked.Read(ref _insertCount)),
            AverageComputeTimeMs: AverageTicks(Interlocked.Read(ref _computeTicks), Interlocked.Read(ref _computeCount)),
            TotalEvictions: Interlocked.Read(ref _evictions),
            ExpiredEvictions: Interlocked.Read(ref _expired),
            CapacityEvictions: Interlocked.Read(ref _capacityEvictions));
    }

    private async Task<object?> ExecuteAndStoreAsync<T>(
        string key,
        Func<CancellationToken, Task<CacheResult<T>>> factory,
        CancellationToken ct)
    {
        var compute = Stopwatch.StartNew();

        try
        {
            var result = await factory(ct);

            if (result.Cacheable)
                StoreValue(key, result.Value);

            return result;
        }
        finally
        {
            compute.Stop();
            Interlocked.Increment(ref _computeCount);
            Interlocked.Add(ref _computeTicks, compute.Elapsed.Ticks);

            _logger.LogDebug("Cache factory completed. Key={Key} DurationMs={DurationMs}",
                key, compute.Elapsed.TotalMilliseconds);

            if (compute.Elapsed.TotalMilliseconds > _options.MissTargetMilliseconds)
            {
                _logger.LogWarning(
                    "Cache miss exceeded the target latency. Key={Key} DurationMs={DurationMs} TargetMs={TargetMs}",
                    key, compute.Elapsed.TotalMilliseconds, _options.MissTargetMilliseconds);
            }
        }
    }

    private void StoreValue<T>(string key, T value)
    {
        var insert = Stopwatch.StartNew();

        var entryOptions = new MemoryCacheEntryOptions
        {
            Size = EntrySize,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.AbsoluteExpirationMinutes),
            SlidingExpiration = _options.SlidingExpirationMinutes > 0
                ? TimeSpan.FromMinutes(_options.SlidingExpirationMinutes)
                : null
        };

        entryOptions.RegisterPostEvictionCallback(OnEvicted);

        _cache.Set(key, value, entryOptions);
        Interlocked.Increment(ref _currentEntries);

        insert.Stop();
        Interlocked.Increment(ref _insertCount);
        Interlocked.Add(ref _insertTicks, insert.Elapsed.Ticks);

        _logger.LogDebug("Cache entry inserted. Key={Key} InsertMs={InsertMs} CurrentEntries={CurrentEntries}",
            key, insert.Elapsed.TotalMilliseconds, Interlocked.Read(ref _currentEntries));
    }

    private void OnEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        Interlocked.Decrement(ref _currentEntries);
        Interlocked.Increment(ref _evictions);

        if (reason == EvictionReason.Expired)
            Interlocked.Increment(ref _expired);

        if (reason == EvictionReason.Capacity)
            Interlocked.Increment(ref _capacityEvictions);

        var remaining = Interlocked.Read(ref _currentEntries);
        if (reason is EvictionReason.Expired or EvictionReason.Capacity or EvictionReason.Removed)
        {
            _logger.LogInformation(
                "Cache entry evicted. Key={Key} Reason={Reason} CurrentEntries={CurrentEntries}",
                key, reason, remaining);
        }
    }

    private void LogRetrieval(string key, TimeSpan elapsed, bool cacheHit)
    {
        _logger.LogDebug("Cache lookup. Key={Key} Hit={Hit} LookupMs={LookupMs}",
            key, cacheHit, elapsed.TotalMilliseconds);

        if (cacheHit && elapsed.TotalMilliseconds > _options.HitTargetMilliseconds)
        {
            _logger.LogWarning(
                "Cache hit exceeded the target latency. Key={Key} LookupMs={LookupMs} TargetMs={TargetMs}",
                key, elapsed.TotalMilliseconds, _options.HitTargetMilliseconds);
        }
    }

    private static double AverageTicks(long ticks, long count)
    {
        return count == 0 ? 0 : ticks / (double)TimeSpan.TicksPerMillisecond / count;
    }
}
