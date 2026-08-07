using System.Collections.Concurrent;
using System.Diagnostics;
using FreeGency.AI.Interfaces;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ReviewModeration.Caching;

/// <summary>
/// The default <see cref="IReviewModerationCache"/>. Storage is delegated to the
/// shared <see cref="IAICacheService"/> (registered once by the AI foundation),
/// so the review module reuses the existing cache instead of duplicating it. Adds
/// per-key single-flight execution (a <see cref="Lazy{T}"/> per key) so concurrent
/// identical requests trigger one AI call and one write, and records cache
/// hit/miss telemetry on the <see cref="IReviewModerationMetrics"/>.
/// </summary>
public sealed class ReviewModerationCache : IReviewModerationCache
{
    private readonly IAICacheService _cache;
    private readonly ReviewModerationCacheOptions _options;
    private readonly IReviewModerationMetrics _metrics;
    private readonly ILogger<ReviewModerationCache> _logger;
    private readonly ConcurrentDictionary<string, Lazy<Task<ReviewModerationResponse?>>> _inflight = new(StringComparer.Ordinal);

    public ReviewModerationCache(
        IAICacheService cache,
        IOptions<ReviewModerationCacheOptions> options,
        IReviewModerationMetrics metrics,
        ILogger<ReviewModerationCache> logger)
    {
        _cache = cache;
        _options = options.Value;
        _metrics = metrics;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReviewModerationResponse?> GetOrAddAsync(
        string key,
        Func<CancellationToken, Task<ReviewModerationResponse>> factory,
        Func<ReviewModerationResponse, bool>? shouldStore = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var cached = await TryGetAsync(key, ct);
        if (cached is not null)
            return cached;

        var flight = _inflight.GetOrAdd(
            key,
            k => new Lazy<Task<ReviewModerationResponse?>>(
                () => ExecuteAsync(key, factory, shouldStore, ct),
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return await flight.Value.ConfigureAwait(false);
        }
        finally
        {
            _inflight.TryRemove(new KeyValuePair<string, Lazy<Task<ReviewModerationResponse?>>>(key, flight));
        }
    }

    /// <inheritdoc />
    public async Task<ReviewModerationResponse?> TryGetAsync(string key, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var cached = await _cache.GetAsync<ReviewModerationResponse>(key, ct);
            if (cached is not null)
            {
                if (_options.EnableMetrics)
                    _metrics.RecordCacheHit(stopwatch.Elapsed);
                return cached;
            }

            if (_options.EnableMetrics)
                _metrics.RecordCacheMiss(stopwatch.Elapsed);
            return null;
        }
        catch (Exception ex)
        {
            if (_options.EnableMetrics)
                _metrics.RecordCacheMiss(stopwatch.Elapsed);

            _logger.LogWarning(
                "Review moderation cache lookup failed. Key={Key} Error={Error}",
                key, ex.Message);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, ReviewModerationResponse response, CancellationToken ct = default)
    {
        var sliding = TimeSpan.FromMinutes(Math.Max(1, _options.SlidingExpirationMinutes));
        var absolute = TimeSpan.FromHours(Math.Max(1, _options.AbsoluteExpirationHours));

        try
        {
            await _cache.SetSlidingAsync(key, response, sliding, absolute, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Review moderation cache write failed. Key={Key} Error={Error}",
                key, ex.Message);
        }
    }

    /// <inheritdoc />
    public Task InvalidateAsync(string key, CancellationToken ct = default)
        => _cache.InvalidateAsync(key, ct);

    /// <inheritdoc />
    public Task<long> CountAsync(CancellationToken ct = default)
        => _cache.CountAsync(ct);

    private async Task<ReviewModerationResponse?> ExecuteAsync(
        string key,
        Func<CancellationToken, Task<ReviewModerationResponse>> factory,
        Func<ReviewModerationResponse, bool>? shouldStore,
        CancellationToken ct)
    {
        var recheck = await TryGetAsync(key, ct);
        if (recheck is not null)
            return recheck;

        var response = await factory(ct).ConfigureAwait(false);

        if (shouldStore is null || shouldStore(response))
            await SetAsync(key, response, ct);

        return response;
    }
}
