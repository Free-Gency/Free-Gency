using System.Security.Cryptography;
using System.Text;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Memory cache for moderation results. Keys are SHA-256 hashes of the normalized
/// message plus the context that can influence the verdict. Thread-safe.
/// </summary>
public sealed class ChatSecurityCache
{
    private const string CacheKeyPrefix = "ai:chat-security:";
    private const string HashSeparator = "|";

    private readonly IMemoryCache _cache;
    private readonly MessageNormalizer _normalizer;

    public ChatSecurityCache(IMemoryCache cache, MessageNormalizer normalizer)
    {
        _cache = cache;
        _normalizer = normalizer;
    }

    public string BuildKey(
        string normalizedMessage,
        ContentType contentType,
        ConversationType conversationType,
        ChatModerationRequest request)
    {
        var canonical = new StringBuilder();

        canonical.Append(normalizedMessage);
        canonical.Append(HashSeparator).Append(contentType);
        canonical.Append(HashSeparator).Append(conversationType);
        canonical.Append(HashSeparator).Append(request.SenderRole);
        canonical.Append(HashSeparator).Append(request.ReceiverRole);

        if (request.PreviousMessages is { Count: > 0 })
        {
            foreach (var previous in request.PreviousMessages)
            {
                canonical.Append(HashSeparator).Append(_normalizer.Normalize(previous));
            }
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
        return CacheKeyPrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool TryGet(string key, out ChatModerationResponse? response)
    {
        return _cache.TryGetValue(key, out response);
    }

    public void Set(string key, ChatModerationResponse response, TimeSpan lifetime)
    {
        _cache.Set(key, response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lifetime,
            Size = 1
        });
    }
}
