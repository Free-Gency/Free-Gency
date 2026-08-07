using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;
using FreeGency.AI.ChatModeration.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Cache-first facade over <see cref="ChatModerationService"/>. It searches the
/// <see cref="IChatModerationCache"/> before calling the AI, stores sanitized
/// results on a miss, and never caches manual-review outcomes. Thread-safe and
/// never throws (delegates the failure policy to the inner service).
/// </summary>
public sealed class CachedChatModerationService : IChatModerationService
{
    private readonly ChatModerationService _inner;
    private readonly IChatModerationCache _cache;
    private readonly ChatSecurityCacheKeyBuilder _keyBuilder;
    private readonly ChatSecurityResponseSanitizer _sanitizer;
    private readonly MessageNormalizer _normalizer;
    private readonly ContentLanguageDetector _languageDetector;
    private readonly ChatModerationCacheOptions _options;
    private readonly ILogger<CachedChatModerationService> _logger;

    public CachedChatModerationService(
        ChatModerationService inner,
        IChatModerationCache cache,
        ChatSecurityCacheKeyBuilder keyBuilder,
        ChatSecurityResponseSanitizer sanitizer,
        MessageNormalizer normalizer,
        ContentLanguageDetector languageDetector,
        IOptions<ChatModerationCacheOptions> options,
        ILogger<CachedChatModerationService> logger)
    {
        _inner = inner;
        _cache = cache;
        _keyBuilder = keyBuilder;
        _sanitizer = sanitizer;
        _normalizer = normalizer;
        _languageDetector = languageDetector;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatModerationResponse> ModerateAsync(ChatModerationRequest request, CancellationToken ct = default)
    {
        var conversationType = ResolveConversationType(request);
        var normalized = _normalizer.Normalize(request.Message);
        var language = _languageDetector.Detect(normalized);
        var key = _keyBuilder.BuildKey(
            normalized,
            conversationType,
            language,
            request.ProjectId,
            ChatSecurityCacheConstants.PromptVersion);

        if (!_options.CacheEnabled)
        {
            _logger.LogDebug("Moderation cache is disabled. Key={Key}", key);
            return await _inner.ModerateAsync(request, ct);
        }

        var result = await _cache.GetOrAddAsync(key, async token =>
        {
            var produced = await _inner.ModerateAsync(request, token);

            if (produced.Action == ModerationAction.ManualReview)
            {
                _logger.LogDebug("Moderation result is not cacheable. Key={Key}", key);
                return new CacheResult<ChatModerationResponse>(produced, Cacheable: false);
            }

            var sanitized = _sanitizer.Sanitize(produced);
            return new CacheResult<ChatModerationResponse>(sanitized, Cacheable: true);
        }, ct);

        if (result.FromCache)
        {
            _logger.LogDebug("Chat moderation cache hit. Key={Key} Action={Action}",
                key, result.Value.Action);
            return result.Value with { UsedCache = true };
        }

        return result.Value;
    }

    public async Task<bool> IsSafeAsync(string message, CancellationToken ct = default)
    {
        var response = await ModerateAsync(new ChatModerationRequest { Message = message }, ct);
        return response.IsSafe;
    }

    public async Task<double> CalculateRiskScoreAsync(ChatModerationRequest request, CancellationToken ct = default)
    {
        var response = await ModerateAsync(request, ct);
        return response.RiskScore;
    }

    private static ConversationType ResolveConversationType(ChatModerationRequest request)
    {
        if (request.Metadata is not null &&
            request.Metadata.TryGetValue("ConversationType", out var value) &&
            Enum.TryParse<ConversationType>(value, ignoreCase: true, out var conversationType))
        {
            return conversationType;
        }

        return ConversationType.PrivateChat;
    }
}
