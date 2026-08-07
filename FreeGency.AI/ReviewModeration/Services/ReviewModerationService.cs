using System.Diagnostics;
using FreeGency.AI.Guardrails;
using FreeGency.AI.Interfaces;
using FreeGency.AI.Moderation;
using FreeGency.AI.Moderation.Reliability;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Models;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Observability;
using FreeGency.AI.ReviewModeration.Prompts;
using FreeGency.AI.ReviewModeration.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ReviewModeration.Services;

/// <summary>
/// The default <see cref="IReviewModerationService"/> implementation. Receives a
/// request, validates and normalizes it, and serves it through the shared AI
/// cache: identical reviews are returned in milliseconds without calling the LLM,
/// while cache misses run the deterministic security and intelligence analysis
/// and delegate to the <see cref="IReviewModerationProvider"/>. Results are merged
/// into a client-safe <see cref="ReviewModerationResponse"/> and, when trusted,
/// stored with sliding/absolute expiration. Never throws to callers; failures
/// fall back to <see cref="ReviewAction.ManualReview"/> while still surfacing the
/// deterministic findings.
/// </summary>
public sealed class ReviewModerationService : IReviewModerationService
{
    private static readonly ReviewSecurityResult EmptySecurityResult = new([], [], [], 0, null);

    private readonly IReviewModerationProvider _provider;
    private readonly IReviewSecurityAnalyzer _securityAnalyzer;
    private readonly IReviewIntelligenceAnalyzer _intelligenceAnalyzer;
    private readonly IReviewModerationCache _cache;
    private readonly ReviewModerationCacheKeyBuilder _cacheKeyBuilder;
    private readonly ModerationOptions _options;
    private readonly ReviewModerationCacheOptions _reviewOptions;
    private readonly IReviewModerationMetrics _metrics;
    private readonly IReviewModerationLogger _moderationLogger;
    private readonly ILogger<ReviewModerationService> _logger;

    public ReviewModerationService(
        IReviewModerationProvider provider,
        IReviewSecurityAnalyzer securityAnalyzer,
        IReviewIntelligenceAnalyzer intelligenceAnalyzer,
        IReviewModerationCache cache,
        ReviewModerationCacheKeyBuilder cacheKeyBuilder,
        IOptions<ModerationOptions> options,
        IOptions<ReviewModerationCacheOptions> reviewOptions,
        IReviewModerationMetrics metrics,
        IReviewModerationLogger moderationLogger,
        ILogger<ReviewModerationService> logger)
    {
        _provider = provider;
        _securityAnalyzer = securityAnalyzer;
        _intelligenceAnalyzer = intelligenceAnalyzer;
        _cache = cache;
        _cacheKeyBuilder = cacheKeyBuilder;
        _options = options.Value;
        _reviewOptions = reviewOptions.Value;
        _metrics = metrics;
        _moderationLogger = moderationLogger;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReviewModerationResponse> ModerateAsync(ReviewModerationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_reviewOptions.EnableMetrics)
            _metrics.RecordRequest();

        var stopwatch = Stopwatch.StartNew();

        if (!ReviewModerationValidator.IsValid(request, out var validationError))
        {
            var invalidIntelligence = _intelligenceAnalyzer.Analyze(
                request.ReviewText ?? string.Empty,
                request.Rating,
                request.PreviousReviews);

            _logger.LogWarning(
                "Review moderation request is invalid. Error={Error} ReviewId={ReviewId}",
                ReviewModerationLogging.Sanitize(validationError),
                request.ReviewId);

            return ToResponse(
                ReviewAnalysisResult.ManualReview(
                    validationError ?? "The review request is invalid.",
                    ReviewModerationSystemPrompt.CurrentVersion,
                    _options.ModelId),
                EmptySecurityResult,
                invalidIntelligence,
                fromCache: false,
                stopwatch.ElapsedMilliseconds,
                _reviewOptions.PromptVersion);
        }

        var normalized = NormalizeRequest(request);
        var cacheKey = BuildCacheKey(normalized);

        if (_reviewOptions.EnableCache)
        {
            var cached = await _cache.TryGetAsync(cacheKey, ct);
            if (cached is not null)
            {
                if (_reviewOptions.EnableLogging)
                {
                    _logger.LogInformation(
                        "Review moderation cache hit. Key={Key} Language={Language} PromptVersion={PromptVersion} Model={Model} ProcessingTimeMs={ProcessingTimeMs}",
                        cacheKey,
                        LogLanguage(normalized.Language),
                        _reviewOptions.PromptVersion,
                        _options.ModelId,
                        stopwatch.ElapsedMilliseconds);
                }

                return cached with
                {
                    FromCache = true,
                    ProcessingTime = stopwatch.ElapsedMilliseconds
                };
            }
        }

        ReviewModerationResponse response;
        if (_reviewOptions.EnableCache)
        {
            response = (await _cache.GetOrAddAsync(
                    cacheKey,
                    BuildFactory(normalized, request, stopwatch),
                    ShouldStore,
                    ct))
                ?? BuildUnavailableResponse(stopwatch);
        }
        else
        {
            response = await BuildFactory(normalized, request, stopwatch)(ct);
        }

        return response;
    }

    private ReviewModerationResponse BuildUnavailableResponse(Stopwatch stopwatch)
        => ToResponse(
            ReviewAnalysisResult.ManualReview(
                "Review moderation is temporarily unavailable.",
                ReviewModerationSystemPrompt.CurrentVersion,
                _options.ModelId),
            EmptySecurityResult,
            ReviewIntelligenceResult.None,
            fromCache: false,
            stopwatch.ElapsedMilliseconds,
            _reviewOptions.PromptVersion);

    private Func<CancellationToken, Task<ReviewModerationResponse>> BuildFactory(
        ReviewModerationRequest normalized,
        ReviewModerationRequest original,
        Stopwatch stopwatch)
    {
        return async innerCt =>
        {
            var security = _securityAnalyzer.Analyze(normalized.ReviewText, original.PreviousReviews);
            var intelligence = _intelligenceAnalyzer.Analyze(
                normalized.ReviewText,
                original.Rating,
                original.PreviousReviews);

            return await AnalyzeAndBuildAsync(normalized, security, intelligence, stopwatch, innerCt);
        };
    }

    private async Task<ReviewModerationResponse> AnalyzeAndBuildAsync(
        ReviewModerationRequest normalized,
        ReviewSecurityResult security,
        ReviewIntelligenceResult intelligence,
        Stopwatch stopwatch,
        CancellationToken ct)
    {
        RecordGuardrailObservability(security);

        try
        {
            var aiStopwatch = Stopwatch.StartNew();
            ReviewAnalysisResult analysis;
            try
            {
                analysis = await _provider.AnalyzeAsync(normalized, ct);
            }
            finally
            {
                aiStopwatch.Stop();
            }

            if (_reviewOptions.EnableMetrics)
                _metrics.RecordAiCall(aiStopwatch.Elapsed);

            var response = ToResponse(
                analysis,
                security,
                intelligence,
                fromCache: false,
                stopwatch.ElapsedMilliseconds,
                _reviewOptions.PromptVersion);

            if (_reviewOptions.EnableLogging)
            {
                _logger.LogInformation(
                    "Review moderation cache miss. ReviewLength={ReviewLength} Language={Language} PromptVersion={PromptVersion} Model={Model} AiTimeMs={AiTimeMs} ProcessingTimeMs={ProcessingTimeMs} RiskScore={RiskScore} Action={Action}",
                    normalized.ReviewText.Length,
                    LogLanguage(normalized.Language),
                    _reviewOptions.PromptVersion,
                    _options.ModelId,
                    aiStopwatch.ElapsedMilliseconds,
                    stopwatch.ElapsedMilliseconds,
                    response.RiskScore,
                    response.Action);
            }

            return response;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Review moderation was cancelled.");
            return ToResponse(
                ReviewAnalysisResult.ManualReview(
                    "Review moderation was cancelled.",
                    ReviewModerationSystemPrompt.CurrentVersion,
                    _options.ModelId),
                security,
                intelligence,
                fromCache: false,
                stopwatch.ElapsedMilliseconds,
                _reviewOptions.PromptVersion);
        }
        catch (ModerationProviderException ex)
        {
            _logger.LogWarning(
                "Review moderation provider failed. Kind={Kind} Message={Message}",
                ex.Kind, ReviewModerationLogging.Sanitize(ex.Message));
            return ToResponse(
                ReviewAnalysisResult.ManualReview(
                    "Review moderation is temporarily unavailable.",
                    ReviewModerationSystemPrompt.CurrentVersion,
                    _options.ModelId),
                security,
                intelligence,
                fromCache: false,
                stopwatch.ElapsedMilliseconds,
                _reviewOptions.PromptVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure during review moderation.");
            return ToResponse(
                ReviewAnalysisResult.ManualReview(
                    "Review moderation is temporarily unavailable.",
                    ReviewModerationSystemPrompt.CurrentVersion,
                    _options.ModelId),
                security,
                intelligence,
                fromCache: false,
                stopwatch.ElapsedMilliseconds,
                _reviewOptions.PromptVersion);
        }
    }

    private static ReviewModerationRequest NormalizeRequest(ReviewModerationRequest request)
        => request with { ReviewText = ReviewModerationValidator.Normalize(request.ReviewText) };

    private void RecordGuardrailObservability(ReviewSecurityResult security)
    {
        if (security.Guardrails is not { } guardrails)
            return;

        if (_reviewOptions.EnableMetrics)
        {
            var result = guardrails.Result;

            if (result.PromptInjectionDetected)
                _metrics.RecordGuardrailSignal("prompt-injection");

            if (result.LlmAbuseDetected)
                _metrics.RecordGuardrailSignal("llm-abuse");

            foreach (var finding in result.SensitiveData)
                _metrics.RecordGuardrailSignal($"sensitive:{finding.Kind}");

            foreach (var finding in result.Profanity)
                _metrics.RecordGuardrailSignal($"profanity:{finding.Style}");

            foreach (var category in result.Toxicity)
                _metrics.RecordGuardrailSignal($"toxicity:{category}");

            foreach (var category in result.Scams)
                _metrics.RecordGuardrailSignal($"scam:{category}");

            if (result.AdvertisementDetected)
                _metrics.RecordGuardrailSignal("advertisement");

            foreach (var signal in result.SpamSignals)
                _metrics.RecordGuardrailSignal($"spam:{signal}");
        }

        if (_reviewOptions.EnableLogging && guardrails.Result.HasAnySignal)
            _moderationLogger.LogGuardrails(guardrails);
    }

    /// <summary>
    /// Builds the versioned cache key: a SHA-256 hash of the normalized review
    /// text, rating, language, prompt version, model version, and moderation
    /// version. Changing any version component invalidates existing entries.
    /// </summary>
    private string BuildCacheKey(ReviewModerationRequest request)
        => _cacheKeyBuilder.BuildKey(
            request.ReviewText,
            request.Rating,
            request.Language,
            _reviewOptions.PromptVersion,
            _options.ModelId,
            ReviewModerationSystemPrompt.CurrentVersion);

    private static string LogLanguage(string? language)
        => string.IsNullOrWhiteSpace(language) ? "unknown" : language.Trim();

    private static bool ShouldStore(ReviewModerationResponse response)
        => response.Action != ReviewAction.ManualReview;

    /// <summary>
    /// Applies the deterministic guardrail business rules on top of the AI verdict:
    /// prompt injection, LLM abuse, and scam are rejected immediately; severe
    /// toxicity (violence, threat, extremism) is rejected; sensitive data and
    /// any toxicity are masked; advertisement and spam are at least warned, with
    /// spam escalated to rejection when the risk score is high. A low-confidence
    /// manual review is never downgraded by the softer rules, but the hard
    /// rejection rules still win.
    /// </summary>
    private static ReviewAction ApplyGuardrailRules(ReviewAction action, ReviewSecurityResult security, double riskScore)
    {
        var categories = security.SecurityCategories;
        var toxicity = security.Guardrails?.Result.Toxicity ?? [];

        if (categories.Contains(ReviewSecurityCategory.PromptInjection))
            return ReviewAction.Reject;

        if (categories.Contains(ReviewSecurityCategory.LlmAbuse))
            return ReviewAction.Reject;

        if (categories.Contains(ReviewSecurityCategory.Scam))
            return ReviewAction.Reject;

        if (toxicity.Any(c => c is ToxicityCategory.Violence or ToxicityCategory.Threat or ToxicityCategory.Extremism))
            return ReviewAction.Reject;

        if (action == ReviewAction.ManualReview)
            return action;

        if (categories.Contains(ReviewSecurityCategory.SensitiveInformation))
            return ReviewAction.Mask;

        if (toxicity.Count > 0)
            return ReviewAction.Mask;

        if (categories.Contains(ReviewSecurityCategory.Spam))
            return riskScore >= 75 ? ReviewAction.Reject : MaxAction(action, ReviewAction.Warn);

        if (categories.Contains(ReviewSecurityCategory.Advertisement))
            return MaxAction(action, ReviewAction.Warn);

        return action;
    }

    private static ReviewAction MaxAction(ReviewAction a, ReviewAction b)
        => a > b ? a : b;

    private static ReviewModerationResponse ToResponse(
        ReviewAnalysisResult analysis,
        ReviewSecurityResult security,
        ReviewIntelligenceResult deterministicIntelligence,
        bool fromCache,
        long elapsedMs,
        string promptVersion)
    {
        var effectiveRiskScore = Math.Max(analysis.RiskScore, security.DeterministicRiskScore);
        var action = ApplyGuardrailRules(
            ResolveAction(effectiveRiskScore, analysis.Confidence, analysis.Action),
            security,
            effectiveRiskScore);
        var riskLevel = MapRiskLevel(effectiveRiskScore);
        var trusted = action != ReviewAction.ManualReview;
        var intelligence = MergeIntelligence(analysis.Intelligence, deterministicIntelligence, trusted);

        var securityCategories = analysis.SecurityCategories
            .Concat(security.SecurityCategories)
            .Distinct()
            .ToList();

        var detectedCategories = MergeCategories(analysis.DetectedCategories, securityCategories, effectiveRiskScore);

        var detectedKeywords = analysis.DetectedKeywords
            .Concat(security.ExtraKeywords)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var maskedReview = analysis.MaskedReview
            ?? (action == ReviewAction.Mask ? security.MaskedText : null);

        return new ReviewModerationResponse
        {
            Approved = action is ReviewAction.Allow or ReviewAction.Warn or ReviewAction.Mask,
            RiskScore = effectiveRiskScore,
            Confidence = analysis.Confidence,
            RiskLevel = riskLevel,
            Action = action,
            Sentiment = intelligence.Sentiment,
            SentimentConfidence = intelligence.SentimentConfidence,
            QualityScore = intelligence.QualityScore,
            QualityBand = intelligence.QualityBand,
            QualityBreakdown = intelligence.QualityBreakdown,
            Constructive = intelligence.Constructive,
            ConstructivenessScore = intelligence.ConstructivenessScore,
            AuthenticityScore = intelligence.AuthenticityScore,
            RatingConsistency = intelligence.RatingConsistency,
            LengthCategory = intelligence.LengthCategory,
            WritingStyle = intelligence.WritingStyle,
            Language = intelligence.Language == ReviewLanguage.Unknown && security.Guardrails?.Language is { } detected
                ? detected
                : intelligence.Language,
            ToxicityScore = analysis.ToxicityScore,
            Summary = intelligence.Summary,
            Reason = analysis.Reason,
            DetectedCategories = detectedCategories,
            SecurityCategories = securityCategories,
            SpamSignals = security.SpamSignals,
            DetectedEntities = analysis.DetectedEntities,
            SuggestedTags = intelligence.Tags,
            Strengths = intelligence.Strengths,
            Weaknesses = intelligence.Weaknesses,
            Recommendation = intelligence.Recommendation,
            DetectedKeywords = detectedKeywords,
            Suggestions = analysis.Suggestions,
            MaskedReview = maskedReview,
            PromptVersion = promptVersion,
            ModelName = analysis.ModelName,
            ProcessingTime = elapsedMs,
            FromCache = fromCache
        };
    }

    /// <summary>
    /// Merges the AI-provided intelligence with the deterministic intelligence.
    /// When the AI produced no understanding (the <see cref="ReviewIntelligenceResult.None"/>
    /// sentinel) or the verdict is not trusted (escalated to manual review), the
    /// deterministic values win entirely. Otherwise the AI values win, falling
    /// back to the deterministic values for anything the model left empty. Tags
    /// are always padded to at least 3 and capped at 7.
    /// </summary>
    private static ReviewIntelligenceResult MergeIntelligence(
        ReviewIntelligenceResult ai,
        ReviewIntelligenceResult deterministic,
        bool trusted)
    {
        if (!trusted || ReferenceEquals(ai, ReviewIntelligenceResult.None))
            return deterministic;

        return new ReviewIntelligenceResult(
            ai.Sentiment,
            ai.SentimentConfidence,
            ai.QualityScore,
            ai.QualityBand,
            ai.QualityBreakdown ?? deterministic.QualityBreakdown,
            ai.Constructive,
            ai.ConstructivenessScore,
            ai.AuthenticityScore,
            ai.RatingConsistency == ReviewRatingConsistency.Unknown
                ? deterministic.RatingConsistency
                : ai.RatingConsistency,
            ai.LengthCategory == ReviewLengthCategory.Normal && deterministic.LengthCategory != ReviewLengthCategory.Normal
                ? deterministic.LengthCategory
                : ai.LengthCategory,
            ai.WritingStyle,
            ai.Language == ReviewLanguage.Unknown ? deterministic.Language : ai.Language,
            ai.Summary ?? deterministic.Summary,
            ai.Strengths.Count > 0 ? ai.Strengths : deterministic.Strengths,
            ai.Weaknesses.Count > 0 ? ai.Weaknesses : deterministic.Weaknesses,
            MergeTags(ai.Tags, deterministic.Tags),
            ai.Recommendation);
    }

    private static IReadOnlyList<string> MergeTags(
        IReadOnlyList<string> aiTags,
        IReadOnlyList<string> deterministicTags)
    {
        const int minTags = 3;
        const int maxTags = 7;

        var merged = aiTags.ToList();
        foreach (var tag in deterministicTags)
        {
            if (merged.Count >= maxTags)
                break;

            if (!merged.Contains(tag, StringComparer.OrdinalIgnoreCase))
                merged.Add(tag);
        }

        if (merged.Count < minTags)
        {
            var fallbacks = new[] { "Informative", "Genuine", "Client Review" };
            foreach (var fallback in fallbacks)
            {
                if (merged.Count >= minTags)
                    break;

                if (!merged.Contains(fallback, StringComparer.OrdinalIgnoreCase))
                    merged.Add(fallback);
            }
        }

        if (merged.Count > maxTags)
            merged = merged.Take(maxTags).ToList();

        return merged;
    }

    private static List<ReviewCategoryDto> MergeCategories(
        IReadOnlyList<ReviewCategoryDto> aiCategories,
        IReadOnlyList<ReviewSecurityCategory> securityCategories,
        double riskScore)
    {
        var merged = aiCategories.ToList();
        var existing = new HashSet<string>(aiCategories.Select(c => c.Category), StringComparer.OrdinalIgnoreCase);
        var score = Math.Clamp(riskScore / 100, 0, 1);

        foreach (var category in securityCategories)
        {
            if (existing.Add(category.ToString()))
                merged.Add(new ReviewCategoryDto(category.ToString(), score));
        }

        return merged;
    }

    /// <summary>
    /// Resolves the final enforcement action deterministically. The AI verdict is
    /// advisory only: a low-confidence verdict or an AI-requested manual review is
    /// escalated to <see cref="ReviewAction.ManualReview"/>; otherwise the action is
    /// derived from the risk score bands 0-20 (Allow), 21-50 (Warn), 51-75 (Mask),
    /// 76-100 (Reject).
    /// </summary>
    private static ReviewAction ResolveAction(double riskScore, double confidence, ReviewAction aiAction)
    {
        if (aiAction == ReviewAction.ManualReview || confidence < 0.60)
            return ReviewAction.ManualReview;

        return MapAction(riskScore);
    }

    private static ReviewAction MapAction(double score)
    {
        return score switch
        {
            > 75 => ReviewAction.Reject,
            > 50 => ReviewAction.Mask,
            > 20 => ReviewAction.Warn,
            _ => ReviewAction.Allow
        };
    }

    private static RiskLevel MapRiskLevel(double score)
    {
        return score switch
        {
            <= 20 => RiskLevel.Safe,
            <= 40 => RiskLevel.Low,
            <= 60 => RiskLevel.Medium,
            <= 80 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }
}
