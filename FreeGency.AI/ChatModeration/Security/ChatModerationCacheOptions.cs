using FreeGency.AI.ChatModeration.Constants;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Options for the cache-first moderation cache. All values are optional and
/// fall back to sensible defaults when the configuration section is absent,
/// so the API can run without editing <c>appsettings.json</c>.
/// </summary>
public sealed class ChatModerationCacheOptions
{
    public const string SectionName = "AI:ChatModeration:Cache";

    /// <summary>Gets a value indicating whether the moderation cache is enabled.</summary>
    public bool CacheEnabled { get; set; } = true;

    /// <summary>Gets the absolute lifetime of a cache entry, in minutes.</summary>
    public int AbsoluteExpirationMinutes { get; set; } = ChatSecurityCacheConstants.DefaultAbsoluteExpirationMinutes;

    /// <summary>Gets the sliding lifetime of a cache entry, in minutes. Zero disables sliding expiry.</summary>
    public int SlidingExpirationMinutes { get; set; } = ChatSecurityCacheConstants.DefaultSlidingExpirationMinutes;

    /// <summary>Gets the maximum number of entries kept in memory. Oldest entries are evicted first.</summary>
    public int MaxEntries { get; set; } = ChatSecurityCacheConstants.DefaultMaxEntries;

    /// <summary>Gets the cache-hit latency target in milliseconds.</summary>
    public double HitTargetMilliseconds { get; set; } = ChatSecurityCacheConstants.DefaultHitTargetMilliseconds;

    /// <summary>Gets the cache-miss (compute) latency target in milliseconds.</summary>
    public double MissTargetMilliseconds { get; set; } = ChatSecurityCacheConstants.DefaultMissTargetMilliseconds;
}
