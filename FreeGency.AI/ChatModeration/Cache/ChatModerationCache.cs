using FreeGency.AI.ChatModeration.Constants;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FreeGency.AI.ChatModeration.Cache;

public sealed class ChatModerationCache : IChatModerationCache
{
    private readonly IMemoryCache _cache;

    public ChatModerationCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<ModerationResult?> TryGetAsync(string key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out ModerationResult? cached))
            return Task.FromResult(cached);

        return Task.FromResult<ModerationResult?>(null);
    }

    public Task SetAsync(string key, ModerationResult result, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        expiration ??= TimeSpan.FromMinutes(ChatModerationConstants.DefaultCacheExpirationMinutes);
        _cache.Set(key, result, expiration.Value);
        return Task.CompletedTask;
    }

    public string BuildKey(ModerationRequest request)
    {
        var contentKey = request.ContentType.ToString() + "|" + (request.EntityId ?? string.Empty) + "|" + request.Content;
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(contentKey)));

        return ChatModerationConstants.CacheKeyPrefix + hash;
    }
}
