namespace FreeGency.AI.ReviewModeration.Caching;

using FreeGency.AI.ReviewModeration.Prompts;

/// <summary>
/// Options for the review moderation cache. Bound to the <c>AI:ReviewModeration</c>
/// configuration section. All values fall back to safe defaults when the section
/// is absent, so the module runs without editing <c>appsettings.json</c>.
/// </summary>
public sealed class ReviewModerationCacheOptions
{
    /// <summary>The configuration section name (<c>AI:ReviewModeration</c>).</summary>
    public const string SectionName = "AI:ReviewModeration";

    /// <summary>The current moderation prompt version. Tracks
    /// <see cref="ReviewModerationSystemPrompt.CurrentVersion"/> so it can never
    /// drift from the prompt that is actually shipped. Bumped whenever the
    /// moderation contract or business rules change; it is part of every cache
    /// key, so bumping it automatically invalidates all previously cached
    /// results.</summary>
    public const string CurrentPromptVersion = ReviewModerationSystemPrompt.CurrentVersion;

    /// <summary>Gets or sets whether successful results are cached. Default <c>true</c>.
    /// Part 7 adds <see cref="EnableCaching"/> as the preferred switch (allows the
    /// value to be overridden at the machine level); when both are configured,
    /// <see cref="EnableCaching"/> wins.</summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>Gets or sets whether caching is enabled. When set, it overrides
    /// <see cref="EnableCache"/>; when <c>null</c> (default), <see cref="EnableCache"/>
    /// is used. Bound to the <c>AI:ReviewModeration:EnableCaching</c> key.</summary>
    public bool? EnableCaching { get; set; }

    /// <summary>Gets or sets whether the module health check is enabled. Default <c>true</c>.</summary>
    public bool EnableHealthCheck { get; set; } = true;

    /// <summary>Gets whether caching is enabled, applying the <see cref="EnableCaching"/>
    /// override when present.</summary>
    public bool CacheEnabled => EnableCaching ?? EnableCache;

    /// <summary>Gets or sets how long an entry survives without being read, in minutes. Default 30.</summary>
    public int SlidingExpirationMinutes { get; set; } = 30;

    /// <summary>Gets or sets the maximum lifetime of any cache entry, in hours. Default 24.</summary>
    public int AbsoluteExpirationHours { get; set; } = 24;

    /// <summary>Gets or sets the moderation prompt version included in the cache key.
    /// Defaults to <see cref="CurrentPromptVersion"/>; override it in configuration to
    /// invalidate cached results without a code change.</summary>
    public string PromptVersion { get; set; } = CurrentPromptVersion;

    /// <summary>Gets or sets whether cache and AI telemetry metrics are collected. Default <c>true</c>.</summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>Gets or sets whether review moderation observability logs are emitted. Default <c>true</c>.</summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>Gets or sets the estimated cost of one AI call, in USD, used to
    /// derive the estimated cost saved by cache hits. Default 0.001.</summary>
    public double EstimatedCostPerAiCallUsd { get; set; } = 0.001;

    /// <summary>Gets or sets whether prompt-injection detection runs in the review
    /// pipeline. Null (default) inherits the shared <c>AI:Guardrails</c> value.
    /// Part 8.</summary>
    public bool? EnablePromptInjectionDetection { get; set; }

    /// <summary>Gets or sets whether sensitive-data detection runs in the review
    /// pipeline. Null (default) inherits the shared <c>AI:Guardrails</c> value.
    /// Part 8.</summary>
    public bool? EnableSensitiveDataDetection { get; set; }

    /// <summary>Gets or sets whether spam detection runs in the review pipeline.
    /// Null (default) inherits the shared <c>AI:Guardrails</c> value. Part 8.</summary>
    public bool? EnableSpamDetection { get; set; }

    /// <summary>Gets or sets whether scam detection runs in the review pipeline.
    /// Null (default) inherits the shared <c>AI:Guardrails</c> value. Part 8.</summary>
    public bool? EnableScamDetection { get; set; }

    /// <summary>Gets or sets whether profanity detection runs in the review
    /// pipeline. Null (default) inherits the shared <c>AI:Guardrails</c> value.
    /// Part 8.</summary>
    public bool? EnableProfanityDetection { get; set; }

    /// <summary>Gets or sets whether multi-language detection runs in the review
    /// pipeline. Null (default) inherits the shared <c>AI:Guardrails</c> value.
    /// Part 8.</summary>
    public bool? EnableMultiLanguageDetection { get; set; }

    /// <summary>Gets or sets whether the AI response is validated (required
    /// properties + data-leakage guard) before it is accepted. Default <c>true</c>.
    /// Part 8.</summary>
    public bool? EnableResponseValidation { get; set; }

    /// <summary>Gets whether AI response validation is enabled, applying the
    /// <see cref="EnableResponseValidation"/> override when present.</summary>
    public bool ResponseValidationEnabled => EnableResponseValidation ?? true;
}
