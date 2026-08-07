namespace FreeGency.AI.ChatModeration.RateLimiting;

/// <summary>
/// Outcome of a single rate-limit check: whether the request is allowed and,
/// when rejected, how long the caller must wait before retrying.
/// </summary>
/// <param name="IsAllowed">True when the request is within the configured limits.</param>
/// <param name="RetryAfter">When <paramref name="IsAllowed"/> is false, the remaining wait before the window resets.</param>
public readonly record struct RateLimitDecision(bool IsAllowed, TimeSpan RetryAfter)
{
    /// <summary>Creates a decision that allows the request.</summary>
    public static RateLimitDecision Allowed() => new(true, TimeSpan.Zero);

    /// <summary>Creates a decision that rejects the request with the given wait time.</summary>
    public static RateLimitDecision Rejected(TimeSpan retryAfter) => new(false, retryAfter);
}
