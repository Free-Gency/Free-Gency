using System.Security.Cryptography;
using System.Text;
using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Moderation.Caching;

/// <summary>
/// Caches successful moderation results in <see cref="IMemoryCache"/>. The cache
/// key is a SHA-256 hash of the content, content kind, surrounding context, and
/// the prompt version, so bumping the prompt version invalidates all entries.
/// Results that require manual review are never cached. Thread-safe.
/// </summary>
public sealed class CachingModerationProvider : IModerationProvider
{
    private const string CacheKeyPrefix = "ai:moderation:";
    private const string HashSeparator = "\u001F";

    private readonly IModerationProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly ModerationOptions _options;
    private readonly ILogger<CachingModerationProvider> _logger;
    private readonly ModerationMetrics _metrics;

    public CachingModerationProvider(
        IModerationProvider inner,
        IMemoryCache cache,
        ModerationOptions options,
        ILogger<CachingModerationProvider> logger,
        ModerationMetrics metrics)
    {
        _inner = inner;
        _cache = cache;
        _options = options;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default)
    {
        var key = BuildKey(request);

        if (_cache.TryGetValue(key, out ModerationResult? cached) && cached is { Success: true })
        {
            _metrics.RecordCacheHit();
            _logger.LogInformation("Moderation cache hit. Key={Key}", key);
            return cached with { FromCache = true };
        }

        _metrics.RecordCacheMiss();

        var result = await _inner.AnalyzeAsync(request, ct);

        if (result.Success && !result.RequiresManualReview)
        {
            _cache.Set(key, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes),
                Size = 1
            });
        }

        return result;
    }

    private string BuildKey(ModerationRequest request)
    {
        var canonical = string.Join(
            HashSeparator,
            request.Content,
            request.ContentKind ?? string.Empty,
            request.Context ?? string.Empty,
            _options.PromptVersion);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return CacheKeyPrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
