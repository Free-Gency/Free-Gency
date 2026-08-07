namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// A point-in-time view of the moderation pipeline health.
/// </summary>
/// <param name="AiAvailable">True when the last observed AI outcome was successful.</param>
/// <param name="CacheAvailable">True when the moderation cache is engaged.</param>
/// <param name="AverageLatencyMs">Average total response time in milliseconds.</param>
/// <param name="FailureRate">Percentage of requests that failed.</param>
/// <param name="RetryRate">Percentage of AI calls that required a retry.</param>
/// <param name="Status">"Healthy", "Degraded", or "Unhealthy".</param>
public sealed record ChatModerationHealth(
    bool AiAvailable,
    bool CacheAvailable,
    double AverageLatencyMs,
    double FailureRate,
    double RetryRate,
    string Status);
