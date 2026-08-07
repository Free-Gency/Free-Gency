namespace FreeGency.AI.ReviewModeration.Reliability;

/// <summary>
/// Review-specific reliability options. Bound to the <c>AI:ReviewModeration</c>
/// section. Every value is optional (<c>null</c> when the key is absent) so the
/// review provider can fall back to the shared <c>AI:Moderation</c> settings,
/// keeping deployments that never configure review-specific reliability working
/// unchanged.
/// </summary>
public sealed class ReviewModerationReliabilityOptions
{
    /// <summary>The configuration section name (<c>AI:ReviewModeration</c>).</summary>
    public const string SectionName = "AI:ReviewModeration";

    /// <summary>
    /// Gets or sets the number of retries for transient AI failures. When unset,
    /// the shared <c>AI:Moderation</c> <c>RetryCount</c> is used.
    /// </summary>
    public int? RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the per-request AI timeout in seconds. When unset, the shared
    /// <c>AI:Moderation</c> <c>TimeoutSeconds</c> is used.
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the consecutive failure count that opens the circuit breaker.
    /// When unset, the shared <c>AI:Moderation</c> <c>CircuitBreakerThreshold</c>
    /// is used.
    /// </summary>
    public int? CircuitBreakerThreshold { get; set; }

    /// <summary>
    /// Gets or sets how long an open circuit waits before probing again, in
    /// minutes. When unset, the shared <c>AI:Moderation</c>
    /// <c>CircuitBreakerResetMinutes</c> is used.
    /// </summary>
    public int? CircuitBreakerResetMinutes { get; set; }
}
