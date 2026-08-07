using FreeGency.AI.Monitoring;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Guardrails;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Structured, thread-safe logger for the review moderation pipeline. Wraps
/// <see cref="ModerationStructuredLogger"/> and stamps every line with the audit
/// fields from the current correlation context. Every event is gated on the
/// <c>AI:ReviewModeration:EnableLogging</c> setting. Never logs raw review text,
/// prompt templates, or system prompts.
/// </summary>
public sealed class ReviewModerationLogger : ModerationStructuredLogger, IReviewModerationLogger
{
    private readonly ReviewModerationCacheOptions _options;
    private readonly ReviewModerationMonitoringOptions _monitoring;

    public ReviewModerationLogger(
        ILogger<ReviewModerationLogger> logger,
        ICorrelationContextAccessor correlation,
        IOptions<ReviewModerationCacheOptions> options,
        IOptions<ReviewModerationMonitoringOptions> monitoring)
        : base(logger, correlation)
    {
        _options = options.Value;
        _monitoring = monitoring.Value;
    }

    /// <inheritdoc />
    public void LogModerationStarted(ReviewModerationRequestContext context)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Information, "ReviewModerationStarted", "Review moderation request started.",
            ("ReviewLength", context.ReviewLength),
            ("Language", context.Language ?? "unknown"),
            ("Rating", context.Rating),
            ("PromptVersion", context.PromptVersion));
    }

    /// <inheritdoc />
    public void LogModerationCompleted(ReviewModerationRequestContext context, ReviewModerationResponse response, TimeSpan totalTime)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Information, "ReviewModerationCompleted", "Review moderation request completed.",
            ("RiskScore", response.RiskScore),
            ("Action", response.Action.ToString()),
            ("RiskLevel", response.RiskLevel.ToString()),
            ("Sentiment", response.Sentiment.ToString()),
            ("UsedCache", response.FromCache),
            ("ProcessingTimeMs", totalTime.TotalMilliseconds));
    }

    /// <inheritdoc />
    public void LogCacheHit()
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Information, "ReviewModerationCacheHit", "Review moderation served from cache.");
    }

    /// <inheritdoc />
    public void LogCacheMiss()
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Information, "ReviewModerationCacheMiss", "Review moderation cache miss.");
    }

    /// <inheritdoc />
    public void LogAiStarted()
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Information, "ReviewModerationAiStarted", "Review moderation AI request started.");
    }

    /// <inheritdoc />
    public void LogAiFinished(TimeSpan elapsed)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Information, "ReviewModerationAiFinished", "Review moderation AI request completed.",
            ("AiMs", elapsed.TotalMilliseconds));
    }

    /// <inheritdoc />
    public void LogAiFailed(string? reason, TimeSpan elapsed)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Error, "ReviewModerationAiFailed", "Review moderation AI request failed.",
            ("Reason", reason),
            ("ElapsedMs", elapsed.TotalMilliseconds));
    }

    /// <inheritdoc />
    public void LogRetryStarted(int attempt, int maxRetries)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Warning, "ReviewModerationRetryStarted", "Retrying review moderation AI request.",
            ("Attempt", attempt),
            ("MaxRetries", maxRetries));
    }

    /// <inheritdoc />
    public void LogRetryFailed(int attempt, int maxRetries)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Warning, "ReviewModerationRetryFailed", "A review moderation AI attempt failed and will be retried.",
            ("Attempt", attempt),
            ("MaxRetries", maxRetries));
    }

    /// <inheritdoc />
    public void LogTimeout(TimeSpan elapsed)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Error, "ReviewModerationTimeout", "Review moderation AI request timed out.",
            ("TimeoutMs", elapsed.TotalMilliseconds));
    }

    /// <inheritdoc />
    public void LogManualReview(ReviewModerationRequestContext context, ReviewModerationResponse response)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Warning, "ReviewModerationManualReview", "Review queued for manual review.",
            ("Reason", response.Reason),
            ("RiskScore", response.RiskScore));
    }

    /// <inheritdoc />
    public void LogAudit(ReviewModerationRequestContext context, ReviewModerationResponse response, TimeSpan processingTime)
    {
        if (!_options.EnableLogging || !_monitoring.EnableAudit)
            return;

        var audit = new ReviewModerationAuditRecord(
            context.RequestId,
            context.CorrelationId,
            context.ReviewId,
            context.ProjectId,
            context.ReviewerId,
            context.ReviewedUserId,
            context.ReviewHash,
            context.ReviewLength,
            context.Rating,
            context.Language,
            response.RiskScore,
            response.Action,
            response.Sentiment,
            response.Confidence,
            response.SecurityCategories.ToList(),
            response.DetectedEntities.Count,
            response.DetectedCategories.Count,
            response.FromCache,
            processingTime,
            response.PromptVersion,
            response.ModelName,
            context.Timestamp);

        Log(LogLevel.Information, "ReviewModerationAudit", "Review moderation audit record.",
            ("ReviewId", audit.ReviewId),
            ("ProjectId", audit.ProjectId),
            ("ReviewerId", audit.ReviewerId),
            ("ReviewedUserId", audit.ReviewedUserId),
            ("ReviewHash", audit.ReviewHash),
            ("ReviewLength", audit.ReviewLength),
            ("Rating", audit.Rating),
            ("Language", audit.Language),
            ("RiskScore", audit.RiskScore),
            ("Action", audit.Action.ToString()),
            ("Sentiment", audit.Sentiment.ToString()),
            ("Confidence", audit.Confidence),
            ("SecurityCategories", audit.SecurityCategories),
            ("EntityCount", audit.EntityCount),
            ("CategoryCount", audit.CategoryCount),
            ("UsedCache", audit.UsedCache),
            ("ProcessingTimeMs", audit.ProcessingTime.TotalMilliseconds),
            ("PromptVersion", audit.PromptVersion),
            ("ModelName", audit.ModelName));
    }

    /// <inheritdoc />
    public void LogUnexpectedException(Exception exception)
    {
        if (!_options.EnableLogging)
            return;

        LogError("ReviewModerationUnexpectedException", "Unexpected exception during review moderation.", exception);
    }

    /// <inheritdoc />
    public void LogGuardrails(ReviewGuardrailOutcome outcome)
    {
        if (!_options.EnableLogging)
            return;

        var result = outcome.Result;
        Log(LogLevel.Information, "ReviewModerationGuardrails", "Review guardrail findings.",
            ("RiskContribution", outcome.RiskContribution),
            ("SecurityCategories", outcome.SecurityCategories.Select(c => c.ToString()).ToList()),
            ("Language", outcome.Language?.ToString() ?? "unknown"),
            ("PromptInjection", result.PromptInjectionDetected),
            ("PromptInjectionKinds", result.PromptInjectionKinds.Select(k => k.ToString()).ToList()),
            ("LlmAbuse", result.LlmAbuseDetected),
            ("LlmAbuseSignals", result.LlmAbuseSignals),
            ("SensitiveData", result.SensitiveData.Select(s => $"{s.Kind}:{s.Count}").ToList()),
            ("Profanity", result.Profanity.Select(p => $"{p.Style}:{p.Count}").ToList()),
            ("Toxicity", result.Toxicity.Select(c => c.ToString()).ToList()),
            ("Scams", result.Scams.Select(c => c.ToString()).ToList()),
            ("Advertisement", result.AdvertisementDetected),
            ("SpamSignals", result.SpamSignals.Select(s => s.ToString()).ToList()),
            ("SpamRiskScore", result.SpamRiskScore));
    }

    /// <inheritdoc />
    public void LogResponseValidationRejected(ReviewResponseValidationResult validation)
    {
        if (!_options.EnableLogging)
            return;

        Log(LogLevel.Warning, "ReviewModerationResponseRejected", "Review moderation AI response failed validation.",
            ("HasIncompleteFields", validation.HasIncompleteFields),
            ("HasLeak", validation.HasLeak),
            ("MissingFields", validation.MissingFields),
            ("LeakSignals", validation.LeakSignals),
            ("Reason", validation.Reason));
    }

    /// <inheritdoc />
    public void LogPerformanceAlerts(IReviewModerationMetrics metrics, TimeSpan aiLatency)
    {
        if (!_options.EnableLogging || !_monitoring.EnablePerformanceLogs)
            return;

        var cooldown = TimeSpan.FromSeconds(_monitoring.AlertCooldownSeconds);

        if (aiLatency.TotalSeconds > _monitoring.AiLatencyWarningSeconds)
        {
            EmitCooldown("latency", cooldown, () =>
                Log(LogLevel.Warning, "ReviewModerationLatencyAlert", "Performance alert: AI latency exceeded threshold.",
                    ("AiMs", aiLatency.TotalMilliseconds),
                    ("ThresholdSeconds", _monitoring.AiLatencyWarningSeconds)));
        }

        if (metrics.FailureRate > _monitoring.MaxFailureRate * 100)
        {
            EmitCooldown("failureRate", cooldown, () =>
                Log(LogLevel.Error, "ReviewModerationFailureRateAlert", "Performance alert: failure rate exceeded threshold.",
                    ("FailureRate", metrics.FailureRate),
                    ("ThresholdPercent", _monitoring.MaxFailureRate * 100)));
        }

        if (metrics.TotalRequests > 0 && metrics.CacheHitRate < _monitoring.MinCacheHitRate * 100)
        {
            EmitCooldown("cacheHitRate", cooldown, () =>
                Log(LogLevel.Warning, "ReviewModerationCacheHitRateAlert", "Performance alert: cache hit rate dropped below threshold.",
                    ("CacheHitRate", metrics.CacheHitRate),
                    ("ThresholdPercent", _monitoring.MinCacheHitRate * 100)));
        }
    }
}
