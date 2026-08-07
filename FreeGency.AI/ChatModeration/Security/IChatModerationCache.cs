namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// A thread-safe, dependency-injected cache for moderation results. Works with
/// any value type, so it can be reused for proposal ranking, reviews, comments,
/// profile and portfolio moderation without changing the implementation.
/// </summary>
public interface IChatModerationCache
{
    /// <summary>Tries to read a value from the cache. On success <paramref name="value"/> is the cached value.</summary>
    bool TryGet<T>(string key, out T? value);

    /// <summary>Stores a value in the cache under the given key.</summary>
    void Set<T>(string key, T value);

    /// <summary>Removes the entry (and any in-flight computation) for the given key.</summary>
    void Remove(string key);

    /// <summary>
    /// Returns the value for <paramref name="key"/> when present; otherwise runs
    /// <paramref name="factory"/> exactly once, even when many callers race on the
    /// same key, and stores the produced value when the factory says it is cacheable.
    /// </summary>
    Task<CacheResult<T>> GetOrAddAsync<T>(
        string key,
        Func<CancellationToken, Task<CacheResult<T>>> factory,
        CancellationToken ct = default);

    /// <summary>Returns an immutable snapshot of the cache metrics.</summary>
    ChatModerationCacheStatistics GetStatistics();
}
