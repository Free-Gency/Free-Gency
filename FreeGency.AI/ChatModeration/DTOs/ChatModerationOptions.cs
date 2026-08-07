using FreeGency.AI.ChatModeration.Constants;

namespace FreeGency.AI.ChatModeration.DTOs;

public sealed class ChatModerationOptions
{
    public const string SectionName = "AI:ChatModeration";

    public string ModelId { get; set; } = ChatModerationConstants.DefaultModelId;
    public int MaxTokens { get; set; } = ChatModerationConstants.DefaultMaxTokens;
    public double Temperature { get; set; } = ChatModerationConstants.DefaultTemperature;

    public int MaxContentLength { get; set; } = ChatModerationConstants.DefaultMaxContentLength;
    public int MinContentLength { get; set; } = ChatModerationConstants.DefaultMinContentLength;

    public bool CacheEnabled { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = ChatModerationConstants.DefaultCacheExpirationMinutes;

    public int MaxConcurrentRequests { get; set; } = ChatModerationConstants.DefaultMaxConcurrentRequests;
    public int RequestTimeoutSeconds { get; set; } = ChatModerationConstants.DefaultRequestTimeoutSeconds;

    public double FlagThreshold { get; set; } = ChatModerationConstants.FlagThreshold;
    public double BlockThreshold { get; set; } = ChatModerationConstants.BlockThreshold;
}
