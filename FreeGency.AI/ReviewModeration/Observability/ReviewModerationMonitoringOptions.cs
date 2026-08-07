namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Options for the review moderation observability layer. Bound to the
/// <c>AI:ReviewModeration:Logging</c> section. All values are optional and fall
/// back to safe defaults when the section is absent.
/// </summary>
public sealed class ReviewModerationMonitoringOptions
{
    /// <summary>The configuration section name (<c>AI:ReviewModeration:Logging</c>).</summary>
    public const string SectionName = "AI:ReviewModeration:Logging";

    /// <summary>Gets or sets whether an audit record is written for every request. Default <c>true</c>.</summary>
    public bool EnableAudit { get; set; } = true;

    /// <summary>Gets or sets whether performance alert logs are emitted. Default <c>true</c>.</summary>
    public bool EnablePerformanceLogs { get; set; } = true;

    /// <summary>
    /// Gets or sets whether sensitive details (masked review, matched keywords)
    /// may be included in logs. When false, only counts and hashes are logged.
    /// Never enables raw review text.
    /// </summary>
    public bool EnableSensitiveLogs { get; set; } = false;

    /// <summary>Gets or sets the AI latency threshold in seconds above which a warning is logged. Default 2.</summary>
    public double AiLatencyWarningSeconds { get; set; } = 2;

    /// <summary>Gets or sets the failure-rate threshold (0..1) above which an error is logged. Default 0.10.</summary>
    public double MaxFailureRate { get; set; } = 0.10;

    /// <summary>Gets or sets the cache-hit-rate threshold (0..1) below which a warning is logged. Default 0.30.</summary>
    public double MinCacheHitRate { get; set; } = 0.30;

    /// <summary>Gets or sets the risk score (0..100) at or above which a review is flagged as high risk. Default 80.</summary>
    public double HighRiskThreshold { get; set; } = 80;

    /// <summary>Gets or sets the minimum interval between repeated performance alerts, in seconds. Default 60.</summary>
    public int AlertCooldownSeconds { get; set; } = 60;
}
