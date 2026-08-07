using System.Diagnostics;
using FreeGency.AI.Monitoring;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Services;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Observability decorator for the review moderation pipeline. Wraps an inner
/// <see cref="ReviewModerationService"/> and, for every request, establishes the
/// correlation context, emits the structured event log (started, completed, cache
/// hit/miss, manual review), writes audit records, records metrics (action,
/// success/failure, total latency, security categories, cache entries), and
/// evaluates performance alerts. It never modifies the moderation outcome and
/// never throws; failures are handled by the inner service.
/// </summary>
public sealed class MonitoredReviewModerationService : IReviewModerationService
{
    private readonly ReviewModerationService _inner;
    private readonly IReviewModerationLogger _logger;
    private readonly ReviewModerationMetrics _metrics;
    private readonly ICorrelationContextAccessor _correlation;
    private readonly IReviewModerationCache _cache;
    private readonly ReviewModerationCacheOptions _options;
    private readonly ReviewModerationMonitoringOptions _monitoring;

    public MonitoredReviewModerationService(
        ReviewModerationService inner,
        IReviewModerationLogger logger,
        ReviewModerationMetrics metrics,
        ICorrelationContextAccessor correlation,
        IReviewModerationCache cache,
        IOptions<ReviewModerationCacheOptions> options,
        IOptions<ReviewModerationMonitoringOptions> monitoring)
    {
        _inner = inner;
        _logger = logger;
        _metrics = metrics;
        _correlation = correlation;
        _cache = cache;
        _options = options.Value;
        _monitoring = monitoring.Value;
    }

    /// <inheritdoc />
    public async Task<ReviewModerationResponse> ModerateAsync(ReviewModerationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var totalStopwatch = Stopwatch.StartNew();
        var context = ReviewModerationRequestContext.FromRequest(request, _options.PromptVersion);
        _correlation.Set(new CorrelationContext(context.RequestId, context.CorrelationId, context.Timestamp));

        if (_options.EnableMetrics)
        {
            _metrics.MarkCacheAvailable(EffectiveCacheEnabled);
            _metrics.RecordCacheEntries(await CountCacheEntriesAsync(ct));
        }

        try
        {
            _logger.LogModerationStarted(context);

            var response = await _inner.ModerateAsync(request, ct);

            totalStopwatch.Stop();
            _logger.LogModerationCompleted(context, response, totalStopwatch.Elapsed);
            LogCache(response);

            if (_options.EnableMetrics)
                RecordMetrics(response, totalStopwatch.Elapsed);

            LogOutcome(context, response);
            _logger.LogAudit(context, response, totalStopwatch.Elapsed);

            if (_monitoring.EnablePerformanceLogs)
            {
                var aiLatency = response.FromCache ? TimeSpan.Zero : totalStopwatch.Elapsed;
                _logger.LogPerformanceAlerts(_metrics, aiLatency);
            }

            return response;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            totalStopwatch.Stop();
            return BuildFallback("Review moderation was cancelled.", totalStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            _logger.LogUnexpectedException(ex);

            if (_options.EnableMetrics)
            {
                _metrics.RecordAction(ReviewAction.ManualReview);
                _metrics.RecordFailure();
                _metrics.MarkAiAvailable(false);
                _metrics.RecordTotalLatency(totalStopwatch.Elapsed);
            }

            return BuildFallback("Review moderation is temporarily unavailable.", totalStopwatch.ElapsedMilliseconds);
        }
        finally
        {
            _correlation.Clear();
        }
    }

    private bool EffectiveCacheEnabled => _options.EnableCaching ?? _options.EnableCache;

    private async Task<long> CountCacheEntriesAsync(CancellationToken ct)
    {
        try
        {
            return await _cache.CountAsync(ct);
        }
        catch
        {
            return 0;
        }
    }

    private void RecordMetrics(ReviewModerationResponse response, TimeSpan totalTime)
    {
        _metrics.RecordTotalLatency(totalTime);
        _metrics.RecordAction(response.Action);

        foreach (var category in response.SecurityCategories)
            _metrics.RecordSecurityCategory(category.ToString());

        _metrics.MarkAiAvailable(response.Action != ReviewAction.ManualReview);

        if (IsFailure(response))
            _metrics.RecordFailure();
        else
            _metrics.RecordSuccess();
    }

    private void LogCache(ReviewModerationResponse response)
    {
        if (response.FromCache)
            _logger.LogCacheHit();
        else
            _logger.LogCacheMiss();
    }

    private void LogOutcome(ReviewModerationRequestContext context, ReviewModerationResponse response)
    {
        if (response.Action == ReviewAction.ManualReview)
            _logger.LogManualReview(context, response);
    }

    private static bool IsFailure(ReviewModerationResponse response)
    {
        if (response.Action != ReviewAction.ManualReview)
            return false;

        var reason = response.Reason ?? string.Empty;
        return reason.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("could not be completed", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("cancelled", StringComparison.OrdinalIgnoreCase);
    }

    private static ReviewModerationResponse BuildFallback(string reason, long elapsedMs)
    {
        return new ReviewModerationResponse
        {
            Approved = false,
            RiskScore = 50,
            Confidence = 0,
            RiskLevel = RiskLevel.Medium,
            Action = ReviewAction.ManualReview,
            Sentiment = ReviewSentiment.Neutral,
            SentimentConfidence = 0,
            QualityScore = 0,
            QualityBand = ReviewQualityBand.Average,
            Constructive = false,
            ConstructivenessScore = 0,
            AuthenticityScore = 0,
            RatingConsistency = ReviewRatingConsistency.Unknown,
            LengthCategory = ReviewLengthCategory.Normal,
            WritingStyle = ReviewWritingStyle.Casual,
            Language = ReviewLanguage.Unknown,
            ToxicityScore = 0,
            Summary = null,
            Reason = reason,
            DetectedCategories = [],
            SecurityCategories = [],
            SpamSignals = [],
            DetectedEntities = [],
            SuggestedTags = [],
            Strengths = [],
            Weaknesses = [],
            Recommendation = ReviewRecommendation.ManualReview,
            DetectedKeywords = [],
            Suggestions = [],
            MaskedReview = null,
            PromptVersion = string.Empty,
            ModelName = null,
            ProcessingTime = elapsedMs,
            FromCache = false
        };
    }
}
