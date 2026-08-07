namespace FreeGency.AI.Interfaces;

public interface IAICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Stores a value that expires a fixed time after the last read (sliding)
    /// and never later than <paramref name="absoluteExpiration"/>. Every
    /// successful read refreshes the sliding window, so hot entries stay cached
    /// as long as they keep being used while cold entries age out. The absolute
    /// bound guarantees entries never outlive the configured maximum.
    /// </summary>
    Task SetSlidingAsync<T>(
        string key,
        T value,
        TimeSpan slidingExpiration,
        TimeSpan? absoluteExpiration = null,
        CancellationToken ct = default) where T : class;

    Task InvalidateAsync(string key, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Returns the approximate number of live entries currently stored. Expired
    /// entries are evicted as part of the count, so the result is the current
    /// working-set size rather than a history of writes.
    /// </summary>
    Task<long> CountAsync(CancellationToken ct = default);
}
