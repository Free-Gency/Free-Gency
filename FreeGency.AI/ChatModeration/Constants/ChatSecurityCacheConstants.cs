namespace FreeGency.AI.ChatModeration.Constants;

/// <summary>
/// Shared constants for the cache-first chat moderation cache (Part 5).
/// </summary>
public static class ChatSecurityCacheConstants
{
    /// <summary>
    /// Bump this whenever the moderation system prompt, scoring rules, or the
    /// response contract change. The value is part of every cache key, so old
    /// cache entries are automatically invalidated on a version bump.
    /// </summary>
    public const string PromptVersion = "1";

    /// <summary>
    /// Prefix of every key managed by the cache-first moderation cache.
    /// </summary>
    public const string CacheKeyPrefix = "ai:chat-moderation:";

    public const int DefaultAbsoluteExpirationMinutes = 15;
    public const int DefaultSlidingExpirationMinutes = 5;
    public const int DefaultMaxEntries = 10_000;
    public const double DefaultHitTargetMilliseconds = 5;
    public const double DefaultMissTargetMilliseconds = 600;
}
