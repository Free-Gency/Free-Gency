namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// A point-in-time view of the review moderation pipeline health.
/// </summary>
/// <param name="AiAvailable">True when the last observed AI outcome was successful.</param>
/// <param name="CacheAvailable">True when the review cache is engaged by configuration.</param>
/// <param name="PromptLoaderAvailable">True when the prompt template and builder load successfully.</param>
/// <param name="ConfigurationAvailable">True when the configured options are valid.</param>
/// <param name="AverageLatencyMs">Average total response time in milliseconds.</param>
/// <param name="FailureRate">Percentage of requests that failed.</param>
/// <param name="RetryRate">Percentage of AI calls that required a retry.</param>
/// <param name="CacheHitRate">Percentage of requests served from cache.</param>
/// <param name="CircuitState">The current circuit breaker state (Closed, Open, HalfOpen).</param>
/// <param name="CacheEntries">The approximate number of entries in the shared AI cache.</param>
/// <param name="Status">"Healthy", "Degraded", or "Unhealthy".</param>
public sealed record ReviewModerationHealth(
    bool AiAvailable,
    bool CacheAvailable,
    bool PromptLoaderAvailable,
    bool ConfigurationAvailable,
    double AverageLatencyMs,
    double FailureRate,
    double RetryRate,
    double CacheHitRate,
    string CircuitState,
    long CacheEntries,
    string Status);
