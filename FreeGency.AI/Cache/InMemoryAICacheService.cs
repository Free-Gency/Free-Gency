using System.Collections.Concurrent;
using FreeGency.AI.Interfaces;

namespace FreeGency.AI.Cache;

/// <summary>
/// Thread-safe, in-memory implementation of <see cref="IAICacheService"/>. Backed
/// by a <see cref="ConcurrentDictionary{TKey,TValue}"/> with atomic writes and an
/// expiring entry per key. Entries support absolute expiration and an optional
/// sliding window that is refreshed on every read. Because the store is local to
/// the process, it is safe under concurrent requests within an instance; scale-out
/// deployments can swap this registration for a distributed <see cref="IAICacheService"/>
/// without changing callers.
/// </summary>
public sealed class InMemoryAICacheService : IAICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new(StringComparer.Ordinal);

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        ct.ThrowIfCancellationRequested();

        if (_store.TryGetValue(key, out var entry) && entry.TouchIfValid())
            return Task.FromResult<T?>(entry.Value as T);

        _store.TryRemove(key, out _);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class
    {
        ct.ThrowIfCancellationRequested();

        _store.AddOrUpdate(
            key,
            _ => new CacheEntry(value, expiration, null),
            (_, _) => new CacheEntry(value, expiration, null));

        return Task.CompletedTask;
    }

    public Task SetSlidingAsync<T>(
        string key,
        T value,
        TimeSpan slidingExpiration,
        TimeSpan? absoluteExpiration = null,
        CancellationToken ct = default) where T : class
    {
        ct.ThrowIfCancellationRequested();

        _store.AddOrUpdate(
            key,
            _ => new CacheEntry(value, absoluteExpiration, slidingExpiration),
            (_, _) => new CacheEntry(value, absoluteExpiration, slidingExpiration));

        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string key, CancellationToken ct = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<long> CountAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var count = 0L;
        foreach (var key in _store.Keys)
        {
            if (_store.TryGetValue(key, out var entry) && entry.TouchIfValid())
            {
                count++;
            }
            else
            {
                _store.TryRemove(key, out _);
            }
        }

        return Task.FromResult(count);
    }

    private sealed class CacheEntry
    {
        private long _lastAccessUtcTicks;

        public object Value { get; }
        public TimeSpan? SlidingExpiration { get; }
        public DateTimeOffset? AbsoluteExpiresAt { get; }

        public CacheEntry(object value, TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            Value = value;
            SlidingExpiration = slidingExpiration;
            var now = DateTimeOffset.UtcNow;
            Interlocked.Exchange(ref _lastAccessUtcTicks, now.UtcTicks);
            AbsoluteExpiresAt = absoluteExpiration.HasValue ? now + absoluteExpiration.Value : null;
        }

        /// <summary>
        /// Returns true when the entry is still live, refreshing the sliding
        /// window on success. Expired entries are reported as invalid so the
        /// caller can evict them.
        /// </summary>
        public bool TouchIfValid()
        {
            var now = DateTimeOffset.UtcNow;

            if (AbsoluteExpiresAt.HasValue && now >= AbsoluteExpiresAt.Value)
                return false;

            if (SlidingExpiration.HasValue)
            {
                var lastAccess = Interlocked.Read(ref _lastAccessUtcTicks);
                if (now.UtcTicks - lastAccess >= SlidingExpiration.Value.Ticks)
                    return false;
            }

            Interlocked.Exchange(ref _lastAccessUtcTicks, now.UtcTicks);
            return true;
        }
    }
}
