namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Options for the observability layer (Part 6). All values are optional and
/// fall back to safe defaults when the configuration section is absent, so the
/// API runs without editing <c>appsettings.json</c>.
/// </summary>
public sealed class ChatModerationMonitoringOptions
{
    public const string SectionName = "AI:ChatModeration:Logging";

    /// <summary>Gets a value indicating whether audit records are written for every request.</summary>
    public bool EnableAudit { get; set; } = true;

    /// <summary>Gets a value indicating whether request metrics are tracked.</summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>Gets a value indicating whether performance alert logs are emitted.</summary>
    public bool EnablePerformanceLogs { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether sensitive details (masked message, matched
    /// keywords, category breakdown) are included in logs. When false, only
    /// counts and hashes are logged. Never enables raw message content.
    /// </summary>
    public bool EnableSensitiveLogs { get; set; } = false;

    /// <summary>Gets the AI latency threshold in seconds above which a warning is logged.</summary>
    public double AiLatencyWarningSeconds { get; set; } = 2;

    /// <summary>Gets the failure-rate threshold (0..1) above which an error is logged.</summary>
    public double MaxFailureRate { get; set; } = 0.10;

    /// <summary>Gets the cache-hit-rate threshold (0..1) below which a warning is logged.</summary>
    public double MinCacheHitRate { get; set; } = 0.30;

    /// <summary>Gets the risk score (0..100) at or above which a message is flagged as high risk.</summary>
    public double HighRiskThreshold { get; set; } = 80;

    /// <summary>Gets the minimum interval between repeated performance alerts, in seconds.</summary>
    public int AlertCooldownSeconds { get; set; } = 60;
}
