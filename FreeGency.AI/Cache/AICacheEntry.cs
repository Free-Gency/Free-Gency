namespace FreeGency.AI.Cache;

public sealed class AICacheEntry<T> where T : class
{
    public required T Value { get; init; }
    public DateTime StoredAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
