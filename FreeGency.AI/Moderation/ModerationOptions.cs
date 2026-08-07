using FreeGency.AI.Moderation.Prompts;

namespace FreeGency.AI.Moderation;

/// <summary>
/// Options for the reusable moderation core. Bound to the <c>AI:Moderation</c>
/// section. All values are optional and fall back to safe defaults when the
/// section is absent, so the API runs without editing <c>appsettings.json</c>.
/// </summary>
public sealed class ModerationOptions
{
    public const string SectionName = "AI:Moderation";

    /// <summary>Gets or sets the model used for moderation.</summary>
    public string ModelId { get; set; } = "meta.llama4-scout-17b-instruct-v1:0";

    /// <summary>Gets or sets the active moderation prompt version. Current value is <c>1.0</c>.</summary>
    public string PromptVersion { get; set; } = Models.PromptVersion.Current.Value;

    /// <summary>Gets or sets the registry name of the active prompt.</summary>
    public string PromptName { get; set; } = ModerationSystemPrompt.DefaultPromptName;

    /// <summary>Gets or sets the maximum number of tokens for the AI completion.</summary>
    public int MaxTokens { get; set; } = 1500;

    /// <summary>Gets or sets the AI sampling temperature.</summary>
    public double Temperature { get; set; } = 0.2;

    /// <summary>Gets or sets the maximum content length accepted by the service.</summary>
    public int MaxContentLength { get; set; } = 10000;

    /// <summary>Gets or sets the per-request AI timeout in seconds (default 10).</summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>Gets or sets the number of retries for transient AI failures (default 1).</summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>Gets or sets the consecutive failure count that opens the circuit breaker (default 5).</summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>Gets or sets how long an open circuit waits before probing again, in minutes (default 1).</summary>
    public int CircuitBreakerResetMinutes { get; set; } = 1;

    /// <summary>Gets or sets whether successful results are cached.</summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>Gets or sets the cache lifetime for successful results, in minutes.</summary>
    public int CacheExpirationMinutes { get; set; } = 15;

    /// <summary>Gets or sets whether the observability/logging decorator is enabled.</summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>Gets or sets whether metrics are collected.</summary>
    public bool EnableMetrics { get; set; } = true;
}
