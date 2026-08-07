using FreeGency.AI.ChatModeration.Interfaces;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.ChatModeration.Logging;

public sealed class ChatModerationLogger : IChatModerationLogger
{
    private readonly ILogger<ChatModerationLogger> _logger;

    public ChatModerationLogger(ILogger<ChatModerationLogger> logger)
    {
        _logger = logger;
    }

    public void LogRequestStarted(string key)
        => _logger.LogInformation("Moderation request started. Key: {Key}", key);

    public void LogRequestCompleted(string key, string decision, TimeSpan elapsed)
        => _logger.LogInformation("Moderation request completed. Key: {Key}, Decision: {Decision}, Elapsed: {Elapsed}ms", key, decision, elapsed.TotalMilliseconds);

    public void LogCacheHit(string key)
        => _logger.LogDebug("Moderation cache hit. Key: {Key}", key);

    public void LogCacheMiss(string key)
        => _logger.LogDebug("Moderation cache miss. Key: {Key}", key);

    public void LogParseFailure(string key)
        => _logger.LogWarning("Failed to parse moderation response. Key: {Key}", key);

    public void LogError(string key, Exception exception)
        => _logger.LogError(exception, "Moderation error. Key: {Key}", key);
}
