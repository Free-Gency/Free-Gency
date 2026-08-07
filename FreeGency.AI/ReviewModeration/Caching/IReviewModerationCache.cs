using FreeGency.AI.ReviewModeration.DTOs;

namespace FreeGency.AI.ReviewModeration.Caching;

/// <summary>
/// Cache access for review moderation results. Backed by the shared
/// <see cref="FreeGency.AI.Interfaces.IAICacheService"/> and extended with
/// per-key single-flight execution so concurrent identical requests never
/// duplicate an AI call or a cache write. Thread-safe and safe to share across
/// requests.
/// </summary>
public interface IReviewModerationCache
{
    /// <summary>
    /// Returns the cached response for <paramref name="key"/>, or runs
    /// <paramref name="factory"/> on a cache miss, stores the produced response
    /// (when <paramref name="shouldStore"/> is null or returns true), and returns
    /// it. Concurrent callers with the same key share a single factory execution.
    /// </summary>
    Task<ReviewModerationResponse?> GetOrAddAsync(
        string key,
        Func<CancellationToken, Task<ReviewModerationResponse>> factory,
        Func<ReviewModerationResponse, bool>? shouldStore = null,
        CancellationToken ct = default);

    /// <summary>Returns the cached response for <paramref name="key"/>, or null on a miss.</summary>
    Task<ReviewModerationResponse?> TryGetAsync(string key, CancellationToken ct = default);

    /// <summary>Stores <paramref name="response"/> under <paramref name="key"/> with the configured sliding/absolute expiration.</summary>
    Task SetAsync(string key, ReviewModerationResponse response, CancellationToken ct = default);

    /// <summary>Removes <paramref name="key"/> from the cache.</summary>
    Task InvalidateAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Returns the approximate number of live entries in the underlying shared
    /// cache (expired entries are evicted as part of the count). Used by the
    /// metrics and health surfaces.
    /// </summary>
    Task<long> CountAsync(CancellationToken ct = default);
}
