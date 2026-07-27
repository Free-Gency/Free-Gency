using System.Collections.Concurrent;
using FreeGency.AI.Interfaces;

namespace FreeGency.AI.Cache;

public sealed class InMemoryAICacheService : IAICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        if (_store.TryGetValue(key, out var entry) && !entry.IsExpired)
            return Task.FromResult<T?>(entry.Value as T);

        _store.TryRemove(key, out _);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class
    {
        var entry = new CacheEntry(value, expiration);
        _store[key] = entry;
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

    private sealed class CacheEntry
    {
        public object Value { get; }
        public DateTimeOffset? ExpiresAt { get; }
        public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt.Value;

        public CacheEntry(object value, TimeSpan? expiration)
        {
            Value = value;
            ExpiresAt = expiration.HasValue ? DateTimeOffset.UtcNow + expiration.Value : null;
        }
    }
}
