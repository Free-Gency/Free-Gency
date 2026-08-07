using FreeGency.AI.Moderation.Reliability;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Reliability;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Default <see cref="IReviewModerationHealthCheck"/>. Combines the observed AI
/// availability and failure rate from the metrics with the live circuit breaker
/// state, the cache configuration, the prompt loader, and the bound options into
/// a single <see cref="ReviewModerationHealth"/> verdict.
/// </summary>
public sealed class ReviewModerationHealthCheck : IReviewModerationHealthCheck
{
    private readonly IReviewModerationProvider _provider;
    private readonly IReviewPromptBuilder _promptBuilder;
    private readonly IReviewModerationMetrics _metrics;
    private readonly ReviewModerationCacheOptions _reviewOptions;
    private readonly ReviewModerationReliabilityOptions _reliability;
    private readonly ReviewModerationMonitoringOptions _monitoring;

    public ReviewModerationHealthCheck(
        IReviewModerationProvider provider,
        IReviewPromptBuilder promptBuilder,
        IReviewModerationMetrics metrics,
        IOptions<ReviewModerationCacheOptions> reviewOptions,
        IOptions<ReviewModerationReliabilityOptions> reliability,
        IOptions<ReviewModerationMonitoringOptions> monitoring)
    {
        _provider = provider;
        _promptBuilder = promptBuilder;
        _metrics = metrics;
        _reviewOptions = reviewOptions.Value;
        _reliability = reliability.Value;
        _monitoring = monitoring.Value;
    }

    /// <inheritdoc />
    public ReviewModerationHealth Evaluate()
    {
        var snapshot = _metrics.GetSnapshot(_reviewOptions.EstimatedCostPerAiCallUsd);

        var configurationAvailable = IsConfigurationValid();
        var promptLoaderAvailable = IsPromptLoaderAvailable();
        var cacheAvailable = _reviewOptions.EnableCaching ?? _reviewOptions.EnableCache;
        var circuitState = _provider.CircuitState;
        var circuitOpen = circuitState == CircuitBreakerState.Open;

        var status = ResolveStatus(snapshot, configurationAvailable, promptLoaderAvailable, circuitOpen);

        return new ReviewModerationHealth(
            AiAvailable: snapshot.AiAvailable,
            CacheAvailable: cacheAvailable,
            PromptLoaderAvailable: promptLoaderAvailable,
            ConfigurationAvailable: configurationAvailable,
            AverageLatencyMs: snapshot.AverageTotalLatencyMs,
            FailureRate: snapshot.FailureRate,
            RetryRate: snapshot.RetryRate,
            CacheHitRate: snapshot.CacheHitRate,
            CircuitState: circuitState.ToString(),
            CacheEntries: snapshot.CacheEntries,
            Status: status);
    }

    private string ResolveStatus(
        ReviewModerationMetricsSnapshot snapshot,
        bool configurationAvailable,
        bool promptLoaderAvailable,
        bool circuitOpen)
    {
        if (!configurationAvailable || !promptLoaderAvailable || circuitOpen)
            return "Unhealthy";

        if (snapshot.TotalRequests > 0 &&
            (snapshot.FailureRate > _monitoring.MaxFailureRate * 100 || !snapshot.AiAvailable))
        {
            return "Unhealthy";
        }

        if (snapshot.TotalRequests > 0 &&
            (snapshot.AverageTotalLatencyMs > _monitoring.AiLatencyWarningSeconds * 1000 ||
             snapshot.CacheHitRate < _monitoring.MinCacheHitRate * 100))
        {
            return "Degraded";
        }

        return "Healthy";
    }

    private bool IsConfigurationValid()
    {
        return !string.IsNullOrWhiteSpace(_reviewOptions.PromptVersion)
            && (_reliability.TimeoutSeconds ?? 10) >= 1
            && (_reliability.CircuitBreakerThreshold ?? 5) >= 1
            && (_reliability.CircuitBreakerResetMinutes ?? 5) >= 1;
    }

    private bool IsPromptLoaderAvailable()
    {
        try
        {
            var prompt = _promptBuilder.Build(new ReviewModerationRequest { ReviewText = "ping" });
            return prompt is not null
                && !string.IsNullOrWhiteSpace(prompt.SystemPrompt)
                && !string.IsNullOrWhiteSpace(prompt.Version);
        }
        catch
        {
            return false;
        }
    }
}
