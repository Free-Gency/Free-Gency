namespace FreeGency.AI.ChatModeration.RateLimiting;

/// <summary>
/// Options for the API rate limiter used by the chat moderation endpoints.
/// Binds optionally from the <c>AI:ChatModeration:RateLimit</c> section;
/// the defaults apply when the section is absent.
/// </summary>
public sealed class ChatModerationRateLimitOptions
{
    public const string SectionName = "AI:ChatModeration:RateLimit";

    /// <summary>Maximum moderation requests allowed per user per minute.</summary>
    public int MaxRequestsPerUserPerMinute { get; set; } = 60;

    /// <summary>Maximum moderation requests allowed per conversation per minute.</summary>
    public int MaxRequestsPerConversationPerMinute { get; set; } = 120;

    /// <summary>Maximum number of tracked rate-limit windows before a cleanup sweep runs.</summary>
    public int MaxTrackedKeys { get; set; } = 100_000;
}
