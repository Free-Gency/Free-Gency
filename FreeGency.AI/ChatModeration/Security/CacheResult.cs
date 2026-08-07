namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// The outcome of producing a cacheable value. <see cref="Cacheable"/> lets the
/// producer decide that a value must be returned to the caller but must never
/// be stored (for example a manual-review or failed moderation result).
/// <see cref="FromCache"/> is set by the cache when a hit is served from memory.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public readonly record struct CacheResult<T>(T Value, bool Cacheable = true, bool FromCache = false);
