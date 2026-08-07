namespace FreeGency.AI.ChatModeration.Constants;

public static class ChatModerationConstants
{
    public const string DefaultModelId = "meta.llama4-scout-17b-instruct-v1:0";
    public const int DefaultMaxTokens = 800;
    public const double DefaultTemperature = 0.2;

    public const int DefaultMaxContentLength = 5000;
    public const int DefaultMinContentLength = 1;

    public const int DefaultCacheExpirationMinutes = 60;
    public const string CacheKeyPrefix = "ai:moderation:";

    public const int DefaultMaxConcurrentRequests = 5;
    public const int DefaultRequestTimeoutSeconds = 30;

    public const double FlagThreshold = 0.5;
    public const double BlockThreshold = 0.8;

    public static readonly string[] SupportedContentTypes =
    [
        "ChatMessage", "Review", "Comment", "ProjectDescription",
        "ProposalCoverLetter", "PortfolioDescription", "UserProfile", "SupportTicket"
    ];
}
