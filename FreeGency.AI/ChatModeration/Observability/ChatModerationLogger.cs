using System.Collections.Concurrent;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Structured, thread-safe logger for the AI moderation pipeline. Wraps
/// <see cref="ILogger{T}"/> and stamps every line with the audit fields from
/// <see cref="ChatModerationRequestContext"/>. Never logs raw message content;
/// messages are represented by SHA-256 hashes and lengths only. Sensitive
/// details are gated behind <see cref="ChatModerationMonitoringOptions.EnableSensitiveLogs"/>.
/// </summary>
public sealed class ChatModerationLogger : IChatModerationLogger
{
    private readonly ILogger<ChatModerationLogger> _logger;
    private readonly ChatModerationMonitoringOptions _options;
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastAlert = new(StringComparer.Ordinal);

    public ChatModerationLogger(ILogger<ChatModerationLogger> logger, IOptions<ChatModerationMonitoringOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public ChatModerationRequestContext BeginRequest(
        ChatModerationRequest request,
        string messageHash,
        ContentLanguage language,
        string promptVersion)
    {
        return new ChatModerationRequestContext(
            RequestId: Guid.NewGuid(),
            CorrelationId: Guid.NewGuid(),
            ConversationId: request.ConversationId,
            ProjectId: request.ProjectId,
            SenderId: request.SenderId,
            ReceiverId: request.ReceiverId,
            MessageHash: messageHash,
            OriginalMessageLength: request.Message?.Length ?? 0,
            Language: language,
            PromptVersion: promptVersion,
            Timestamp: DateTimeOffset.UtcNow);
    }

    public void LogRequestStarted(ChatModerationRequestContext context)
        => Log(LogLevel.Information, "ChatModerationStarted", "Chat moderation request started.", context,
            ("MessageHash", context.MessageHash),
            ("MessageLength", context.OriginalMessageLength),
            ("Language", context.Language.ToString()),
            ("PromptVersion", context.PromptVersion));

    public void LogCacheHit(ChatModerationRequestContext context, TimeSpan lookupTime)
        => Log(LogLevel.Information, "ChatModerationCacheHit", "Chat moderation served from cache.", context,
            ("LookupMs", lookupTime.TotalMilliseconds));

    public void LogCacheMiss(ChatModerationRequestContext context)
        => Log(LogLevel.Information, "ChatModerationCacheMiss", "Chat moderation cache miss.", context);

    public void LogAiStarted(ChatModerationRequestContext context)
        => Log(LogLevel.Information, "ChatModerationAiStarted", "AI moderation call started.", context);

    public void LogAiFinished(ChatModerationRequestContext context, TimeSpan elapsed, bool usedCache)
        => Log(LogLevel.Information, "ChatModerationAiFinished", "AI moderation call finished.", context,
            ("AiMs", elapsed.TotalMilliseconds),
            ("UsedCache", usedCache));

    public void LogAllowed(ChatModerationRequestContext context, ChatModerationResponse response)
        => Log(LogLevel.Information, "ChatModerationAllowed", "Message allowed.", context,
            ("RiskScore", response.RiskScore),
            ("Confidence", response.Confidence),
            ("RiskLevel", response.RiskLevel.ToString()));

    public void LogWarned(ChatModerationRequestContext context, ChatModerationResponse response)
        => Log(LogLevel.Warning, "ChatModerationWarned", "Message warned.", context,
            ("RiskScore", response.RiskScore),
            ("Reason", response.Reason));

    public void LogHighRisk(ChatModerationRequestContext context, ChatModerationResponse response)
        => Log(LogLevel.Warning, "ChatModerationHighRisk", "Message scored high risk.", context,
            ("RiskScore", response.RiskScore),
            ("RiskLevel", response.RiskLevel.ToString()),
            ("Reason", response.Reason));

    public void LogRejected(ChatModerationRequestContext context, ChatModerationResponse response)
        => Log(LogLevel.Warning, "ChatModerationRejected", "Message rejected.", context,
            ("RiskScore", response.RiskScore),
            ("Reason", response.Reason));

    public void LogMasked(ChatModerationRequestContext context, ChatModerationResponse response)
    {
        if (_options.EnableSensitiveLogs)
        {
            Log(LogLevel.Warning, "ChatModerationMasked", "Message masked.", context,
                ("RiskScore", response.RiskScore),
                ("MaskedMessage", response.MaskedMessage));
        }
        else
        {
            Log(LogLevel.Warning, "ChatModerationMasked", "Message masked.", context,
                ("RiskScore", response.RiskScore),
                ("MaskedLength", response.MaskedMessage?.Length ?? 0));
        }
    }

    public void LogManualReview(ChatModerationRequestContext context, ChatModerationResponse response)
        => Log(LogLevel.Warning, "ChatModerationManualReview", "Message queued for manual review.", context,
            ("Reason", response.Reason),
            ("RiskScore", response.RiskScore));

    public void LogTimeout(ChatModerationRequestContext context, TimeSpan elapsed)
        => Log(LogLevel.Error, "ChatModerationTimeout", "Moderation timed out.", context,
            ("TimeoutMs", elapsed.TotalMilliseconds));

    public void LogNetworkFailure(ChatModerationRequestContext context, TimeSpan elapsed)
        => Log(LogLevel.Error, "ChatModerationNetworkFailure", "Network failure during moderation.", context,
            ("ElapsedMs", elapsed.TotalMilliseconds));

    public void LogAiFailure(ChatModerationRequestContext context, string? reason, TimeSpan elapsed)
        => Log(LogLevel.Error, "ChatModerationAiFailure", "AI moderation failed.", context,
            ("Reason", reason),
            ("ElapsedMs", elapsed.TotalMilliseconds));

    public void LogInvalidJson(ChatModerationRequestContext context)
        => Log(LogLevel.Error, "ChatModerationInvalidJson", "AI response could not be parsed as JSON.", context);

    public void LogCancelled(ChatModerationRequestContext context)
        => Log(LogLevel.Warning, "ChatModerationCancelled", "Chat moderation cancelled.", context);

    public void LogUnexpectedException(ChatModerationRequestContext context, Exception exception)
        => LogError(context, "ChatModerationUnexpectedException", "Unexpected exception during moderation.", exception);

    public void LogAudit(ChatModerationRequestContext context, ChatModerationResponse response, TimeSpan processingTime, int retryCount)
    {
        if (!_options.EnableAudit)
            return;

        var audit = new ChatModerationAuditRecord(
            context.RequestId,
            context.CorrelationId,
            context.ConversationId,
            context.ProjectId,
            context.SenderId,
            context.ReceiverId,
            context.MessageHash,
            context.OriginalMessageLength,
            response.MaskedMessage?.Length,
            context.Language,
            response.RiskScore,
            response.Confidence,
            response.RiskLevel,
            response.Action,
            response.Categories.Select(c => c.Category.ToString()).ToList(),
            response.DetectedEntities.Count,
            _options.EnableSensitiveLogs ? response.MatchedKeywords.ToList() : [],
            response.UsedCache,
            retryCount,
            processingTime,
            context.PromptVersion,
            response.ModelName,
            context.Timestamp);

        Log(LogLevel.Information, "ChatModerationAudit", "Chat moderation audit record.", context,
            ("ConversationId", audit.ConversationId),
            ("ProjectId", audit.ProjectId),
            ("SenderId", audit.SenderId),
            ("ReceiverId", audit.ReceiverId),
            ("MessageHash", audit.MessageHash),
            ("OriginalMessageLength", audit.OriginalMessageLength),
            ("MaskedMessageLength", audit.MaskedMessageLength),
            ("Language", audit.Language.ToString()),
            ("RiskScore", audit.RiskScore),
            ("Confidence", audit.Confidence),
            ("RiskLevel", audit.RiskLevel.ToString()),
            ("Action", audit.Action.ToString()),
            ("DetectedCategories", audit.DetectedCategories),
            ("DetectedEntitiesCount", audit.DetectedEntitiesCount),
            ("MatchedKeywords", audit.MatchedKeywords),
            ("KeywordCount", audit.MatchedKeywords.Count),
            ("UsedCache", audit.UsedCache),
            ("RetryCount", audit.RetryCount),
            ("ProcessingTimeMs", audit.ProcessingTime.TotalMilliseconds),
            ("PromptVersion", audit.PromptVersion),
            ("ModelName", audit.ModelName));
    }

    public void LogPerformanceAlerts(ChatModerationMetrics metrics, TimeSpan aiLatency)
    {
        if (!_options.EnablePerformanceLogs)
            return;

        var now = DateTimeOffset.UtcNow;

        if (aiLatency.TotalSeconds > _options.AiLatencyWarningSeconds)
        {
            EmitCooldown("latency", now, () =>
                _logger.LogWarning(
                    "Performance alert: AI latency {AiMs}ms exceeded threshold {ThresholdSeconds}s.",
                    aiLatency.TotalMilliseconds, _options.AiLatencyWarningSeconds));
        }

        var statistics = metrics.GetStatistics();

        if (statistics.FailureRate > _options.MaxFailureRate * 100)
        {
            EmitCooldown("failureRate", now, () =>
                _logger.LogError(
                    "Performance alert: failure rate {FailureRate:F1}% exceeded threshold {ThresholdPercent}%.",
                    statistics.FailureRate, _options.MaxFailureRate * 100));
        }

        if ((statistics.TotalCacheHits + statistics.TotalCacheMisses) > 0 &&
            statistics.CacheHitRate < _options.MinCacheHitRate * 100)
        {
            EmitCooldown("cacheHitRate", now, () =>
                _logger.LogWarning(
                    "Performance alert: cache hit rate {HitRate:F1}% dropped below threshold {ThresholdPercent}%.",
                    statistics.CacheHitRate, _options.MinCacheHitRate * 100));
        }
    }

    private void EmitCooldown(string key, DateTimeOffset now, Action emit)
    {
        var cooldown = TimeSpan.FromSeconds(_options.AlertCooldownSeconds);
        var last = _lastAlert.GetOrAdd(key, now);

        if (now - last < cooldown)
            return;

        if (_lastAlert.TryUpdate(key, now, last))
            emit();
    }

    private void Log(
        LogLevel level,
        string eventName,
        string message,
        ChatModerationRequestContext context,
        params (string Key, object? Value)[] fields)
    {
        var state = new List<KeyValuePair<string, object?>>(fields.Length + 5)
        {
            new("Event", eventName),
            new("RequestId", context.RequestId),
            new("CorrelationId", context.CorrelationId),
            new("ConversationId", context.ConversationId),
            new("TimestampUtc", context.Timestamp)
        };

        foreach (var (key, value) in fields)
            state.Add(new KeyValuePair<string, object?>(key, value));

        _logger.Log(level, 0, new StructuredLogState(message, state), null, static (logState, _) => logState.ToString());
    }

    private void LogError(
        ChatModerationRequestContext context,
        string eventName,
        string message,
        Exception exception)
    {
        var state = new List<KeyValuePair<string, object?>>
        {
            new("Event", eventName),
            new("RequestId", context.RequestId),
            new("CorrelationId", context.CorrelationId),
            new("ConversationId", context.ConversationId),
            new("TimestampUtc", context.Timestamp)
        };

        _logger.Log(LogLevel.Error, 0, new StructuredLogState(message, state), exception, static (logState, _) => logState.ToString());
    }
}
