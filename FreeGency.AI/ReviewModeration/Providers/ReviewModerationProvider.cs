using System.Net;
using FreeGency.AI.Moderation;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Reliability;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Models;
using FreeGency.AI.ReviewModeration.Observability;
using FreeGency.AI.ReviewModeration.Prompts;
using FreeGency.AI.ReviewModeration.Reliability;
using FreeGency.AI.ReviewModeration.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.ReviewModeration.Providers;

/// <summary>
/// The real review moderation provider. Calls the existing AI layer through the
/// registered <see cref="IChatCompletionService"/> (the Bedrock gateway) with the
/// prompt built by <see cref="IReviewPromptBuilder"/> and maps the model's JSON
/// reply into a <see cref="ReviewAnalysisResult"/>. The reply is validated before
/// it is accepted (required properties plus a data-leakage guard); a reply that
/// fails validation is a transient condition that flows through the existing
/// retry policy and falls back to manual review. It also owns the reused
/// <see cref="ModerationCircuitBreaker"/> and applies the configured timeout and
/// retry policy around each AI call. It never contains business rules or validation.
/// </summary>
public sealed class ReviewModerationProvider : IReviewModerationProvider
{
    private readonly IChatCompletionService _chat;
    private readonly IReviewPromptBuilder _promptBuilder;
    private readonly ReviewModerationJsonParser _jsonParser;
    private readonly ReviewModerationResponseValidator _responseValidator;
    private readonly ReviewModerationCacheOptions _reviewOptions;
    private readonly ModerationOptions _options;
    private readonly ModerationCircuitBreaker _circuitBreaker;
    private readonly int _retryCount;
    private readonly TimeSpan _timeout;
    private readonly ILogger<ReviewModerationProvider> _logger;
    private readonly IReviewModerationLogger _moderationLogger;

    public ReviewModerationProvider(
        IChatCompletionService chat,
        IReviewPromptBuilder promptBuilder,
        IOptions<ModerationOptions> options,
        IOptions<ReviewModerationReliabilityOptions> reliability,
        IOptions<ReviewModerationCacheOptions> reviewOptions,
        IReviewModerationLogger moderationLogger,
        ILogger<ReviewModerationProvider> logger)
    {
        _chat = chat;
        _promptBuilder = promptBuilder;
        _options = options.Value;
        _jsonParser = new ReviewModerationJsonParser();
        _responseValidator = new ReviewModerationResponseValidator();
        _reviewOptions = reviewOptions.Value;

        var shared = options.Value;
        var review = reliability.Value;

        var threshold = review.CircuitBreakerThreshold ?? shared.CircuitBreakerThreshold;
        var resetMinutes = review.CircuitBreakerResetMinutes ?? shared.CircuitBreakerResetMinutes;

        _circuitBreaker = new ModerationCircuitBreaker(
            Math.Max(1, threshold),
            TimeSpan.FromMinutes(Math.Max(1, resetMinutes)));
        _retryCount = Math.Max(0, review.RetryCount ?? shared.RetryCount);
        _timeout = TimeSpan.FromSeconds(Math.Max(1, review.TimeoutSeconds ?? shared.TimeoutSeconds));
        _moderationLogger = moderationLogger;
        _logger = logger;
    }

    /// <inheritdoc />
    public CircuitBreakerState CircuitState => _circuitBreaker.State;

    /// <inheritdoc />
    public async Task<ReviewAnalysisResult> AnalyzeAsync(ReviewModerationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_circuitBreaker.IsOpen)
        {
            _logger.LogWarning(
                "Review moderation circuit breaker is open (state={CircuitState}). Skipping the AI call.",
                _circuitBreaker.State);
            throw new ModerationProviderException(
                ModerationFailureKind.Internal,
                "The review moderation circuit is open.",
                isTransient: false);
        }

        ModerationProviderException? lastFailure = null;

        for (var attempt = 0; attempt <= _retryCount; attempt++)
        {
            if (attempt > 0)
            {
                _moderationLogger.LogRetryStarted(attempt, _retryCount);
                _logger.LogWarning(
                    "Retrying review moderation AI call. Attempt={Attempt} MaxRetries={MaxRetries}",
                    attempt, _retryCount);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeout);

            try
            {
                var result = await CallAiOnceAsync(request, cts.Token);
                _circuitBreaker.RecordSuccess();
                return result;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _moderationLogger.LogTimeout(_timeout);
                lastFailure = new ModerationProviderException(
                    ModerationFailureKind.Timeout,
                    "The review moderation AI request timed out.",
                    isTransient: true);
            }
            catch (ModerationProviderException ex) when (ex.IsTransient && attempt < _retryCount)
            {
                if (attempt > 0)
                    _moderationLogger.LogRetryFailed(attempt, _retryCount);

                lastFailure = ex;
            }
            catch (ModerationProviderException ex)
            {
                _circuitBreaker.RecordFailure();
                _logger.LogWarning(
                    "Review moderation provider failed. Kind={Kind} CircuitState={CircuitState}",
                    ex.Kind, _circuitBreaker.State);
                throw;
            }
            catch (Exception ex)
            {
                lastFailure = new ModerationProviderException(
                    ModerationFailureKind.Internal,
                    "The review moderation AI call failed unexpectedly.",
                    isTransient: true,
                    ex);
            }
        }

        _circuitBreaker.RecordFailure();
        throw lastFailure
            ?? new ModerationProviderException(
                ModerationFailureKind.Internal,
                "The review moderation AI call failed.",
                isTransient: true);
    }

    private async Task<ReviewAnalysisResult> CallAiOnceAsync(ReviewModerationRequest request, CancellationToken ct)
    {
        var prompt = _promptBuilder.Build(request);

        var history = new ChatHistory();
        history.AddSystemMessage(prompt.SystemPrompt);
        history.AddUserMessage(prompt.UserPrompt);

        try
        {
            var response = await _chat.GetChatMessageContentsAsync(
                history,
                CreateSettings(),
                cancellationToken: ct);

            var raw = response.FirstOrDefault()?.Content;
            var completionLength = raw?.Length ?? 0;

            if (!_jsonParser.TryParse(raw, out var ai) || ai is null)
            {
                throw new ModerationProviderException(
                    ModerationFailureKind.InvalidJson,
                    "The review moderation AI response could not be parsed as JSON.",
                    isTransient: true);
            }

            if (_reviewOptions.ResponseValidationEnabled)
            {
                var validation = _responseValidator.Validate(raw, ai);
                if (!validation.IsValid)
                {
                    _logger.LogWarning(
                        "Review moderation AI response failed validation. HasIncompleteFields={HasIncompleteFields} HasLeak={HasLeak} MissingFields={MissingFields} LeakSignals={LeakSignals}",
                        validation.HasIncompleteFields,
                        validation.HasLeak,
                        validation.MissingFields,
                        validation.LeakSignals);
                    _moderationLogger.LogResponseValidationRejected(validation);

                    throw new ModerationProviderException(
                        ModerationFailureKind.InvalidJson,
                        validation.Reason ?? "The review moderation AI response failed validation.",
                        isTransient: true);
                }
            }

            return BuildAnalysis(ai, request.ReviewText, request.Rating, completionLength);
        }
        catch (ModerationProviderException)
        {
            throw;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw MapHttpFailure(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure during the review moderation AI call.");
            throw new ModerationProviderException(
                ModerationFailureKind.Internal,
                "The review moderation AI call failed unexpectedly.",
                isTransient: true,
                ex);
        }
    }

    private ReviewAnalysisResult BuildAnalysis(
        ReviewAiResponse ai,
        string reviewText,
        int? rating,
        int completionLength)
    {
        var riskScore = Math.Round(ai.RiskScore is double risk ? Math.Clamp(risk, 0, 100) : DeriveRiskScore(ai));
        var confidence = ai.Confidence is double conf ? Math.Clamp(conf, 0, 1) : 0.5;
        var qualityScore = ai.QualityScore is double quality ? Math.Clamp(quality, 0, 100) : DeriveQualityScore(ai, riskScore);
        var toxicityScore = ai.Toxicity is double toxicity ? Math.Clamp(toxicity, 0, 100) : DeriveToxicity(ai, riskScore);
        var riskLevel = ParseEnum<RiskLevel>(ai.RiskLevel) ?? MapRiskLevel(riskScore);
        var action = ParseEnum<ReviewAction>(ai.Action) ?? MapAction(riskScore);
        var sentiment = ParseEnum<ReviewSentiment>(ai.Sentiment) ?? ReviewSentiment.Neutral;
        var reason = string.IsNullOrWhiteSpace(ai.Reason) ? BuildDefaultReason(action, riskScore) : ai.Reason;

        var categories = ai.Categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .Select(c => new ReviewCategoryDto(c.Category, Math.Clamp(c.Score ?? 0.5, 0, 1)))
            .ToList();

        var securityCategories = ai.SecurityCategories
            .Select(s => ParseEnum<ReviewSecurityCategory>(s))
            .OfType<ReviewSecurityCategory>()
            .Distinct()
            .ToList();

        var entities = ai.Entities
            .Where(e => !string.IsNullOrWhiteSpace(e.Type) && !string.IsNullOrWhiteSpace(e.Value))
            .Select(e => new ReviewEntityDto(e.Type, e.Value, Math.Clamp(e.Confidence ?? 0.5, 0, 1)))
            .ToList();

        var keywords = ai.Keywords
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToList();

        var suggestions = ai.Suggestions
            .Where(s => !string.IsNullOrWhiteSpace(s.Type) && !string.IsNullOrWhiteSpace(s.Message))
            .Take(3)
            .Select(s => new ReviewSuggestionDto(s.Type, s.Message, s.Severity ?? "Medium"))
            .ToList();

        var intelligence = BuildIntelligence(ai, reviewText, rating, confidence, riskScore, sentiment, qualityScore);

        var maskedReview = string.IsNullOrWhiteSpace(ai.MaskedReview) ? null : ai.MaskedReview;

        _logger.LogInformation(
            "Review moderation AI completed. RiskScore={RiskScore} Confidence={Confidence} Action={Action} " +
            "RiskLevel={RiskLevel} Sentiment={Sentiment} QualityScore={QualityScore} ToxicityScore={ToxicityScore} " +
            "AuthenticityScore={AuthenticityScore} Recommendation={Recommendation} " +
            "CategoryCount={CategoryCount} SecurityCategoryCount={SecurityCategoryCount} EntityCount={EntityCount} " +
            "TagCount={TagCount} KeywordCount={KeywordCount} SuggestionCount={SuggestionCount} CompletionLength={CompletionLength}",
            riskScore, confidence, action, riskLevel, sentiment, qualityScore, toxicityScore,
            intelligence.AuthenticityScore, intelligence.Recommendation,
            categories.Count, securityCategories.Count, entities.Count, intelligence.Tags.Count, keywords.Count,
            suggestions.Count, completionLength);

        return new ReviewAnalysisResult(
            riskScore,
            confidence,
            riskLevel,
            action,
            toxicityScore,
            reason,
            categories,
            securityCategories,
            entities,
            keywords,
            suggestions,
            maskedReview,
            intelligence,
            ReviewModerationSystemPrompt.CurrentVersion,
            _options.ModelId);
    }

    private static ReviewIntelligenceResult BuildIntelligence(
        ReviewAiResponse ai,
        string reviewText,
        int? rating,
        double confidence,
        double riskScore,
        ReviewSentiment sentiment,
        double qualityScore)
    {
        var sentimentConfidence = ai.SentimentConfidence is double sc ? Math.Clamp(sc, 0, 1) : confidence;
        var qualityBand = MapQualityBand(qualityScore);
        var qualityBreakdown = MapQualityBreakdown(ai.QualityBreakdown);
        var constructivenessScore = ai.ConstructivenessScore is double cs
            ? Math.Clamp(cs, 0, 100)
            : qualityScore;
        var constructive = ai.Constructive ?? constructivenessScore >= 50;
        var authenticityScore = ai.AuthenticityScore is double auth
            ? Math.Clamp(auth, 0, 100)
            : qualityScore;
        var ratingConsistency = ParseEnum<ReviewRatingConsistency>(ai.RatingConsistency)
            ?? DeriveRatingConsistency(sentiment, rating);
        var lengthCategory = ParseEnum<ReviewLengthCategory>(ai.LengthCategory)
            ?? DeriveLengthCategory(reviewText);
        var writingStyle = ParseEnum<ReviewWritingStyle>(ai.WritingStyle) ?? ReviewWritingStyle.Casual;
        var language = ParseEnum<ReviewLanguage>(ai.Language) ?? ReviewLanguage.Unknown;
        var strengths = CleanList(ai.Strengths, 5);
        var weaknesses = CleanList(ai.Weaknesses, 5);
        var tags = CleanList(ai.SuggestedTags, 7);
        var recommendation = ParseEnum<ReviewRecommendation>(ai.Recommendation)
            ?? DeriveRecommendation(riskScore, qualityScore, authenticityScore, sentiment);
        var summary = BuildSummary(ai);

        return new ReviewIntelligenceResult(
            sentiment,
            sentimentConfidence,
            qualityScore,
            qualityBand,
            qualityBreakdown,
            constructive,
            constructivenessScore,
            authenticityScore,
            ratingConsistency,
            lengthCategory,
            writingStyle,
            language,
            summary,
            strengths,
            weaknesses,
            tags,
            recommendation);
    }

    private static List<string> CleanList(IEnumerable<string> values, int max)
        => values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(max)
            .ToList();

    private static ReviewQualityBreakdownDto? MapQualityBreakdown(ReviewRawQualityBreakdown? raw)
    {
        if (raw is null)
            return null;

        double Value(double? value) => value is double number ? Math.Clamp(number, 0, 100) : 50;

        return new ReviewQualityBreakdownDto(
            Value(raw.Grammar),
            Value(raw.Spelling),
            Value(raw.Readability),
            Value(raw.ProfessionalTone),
            Value(raw.Constructiveness),
            Value(raw.Helpfulness),
            Value(raw.Clarity),
            Value(raw.SpecificDetails),
            Value(raw.Length),
            Value(raw.Relevance),
            Value(raw.Originality));
    }

    private static ReviewQualityBand MapQualityBand(double score)
    {
        return score switch
        {
            >= 90 => ReviewQualityBand.Excellent,
            >= 70 => ReviewQualityBand.Good,
            >= 50 => ReviewQualityBand.Average,
            >= 30 => ReviewQualityBand.Poor,
            _ => ReviewQualityBand.VeryPoor
        };
    }

    private static ReviewRatingConsistency DeriveRatingConsistency(ReviewSentiment sentiment, int? rating)
    {
        if (rating is not int value)
            return ReviewRatingConsistency.Unknown;

        var positive = sentiment is ReviewSentiment.VeryPositive or ReviewSentiment.Positive;
        var negative = sentiment is ReviewSentiment.VeryNegative or ReviewSentiment.Negative;

        if (value >= 4 && negative)
            return ReviewRatingConsistency.Inconsistent;

        if (value <= 2 && positive)
            return ReviewRatingConsistency.Inconsistent;

        return ReviewRatingConsistency.Consistent;
    }

    private static ReviewLengthCategory DeriveLengthCategory(string text)
    {
        var length = text.Trim().Length;
        return length switch
        {
            < 20 => ReviewLengthCategory.VeryShort,
            < 50 => ReviewLengthCategory.Short,
            < 200 => ReviewLengthCategory.Normal,
            < 500 => ReviewLengthCategory.Detailed,
            _ => ReviewLengthCategory.VeryDetailed
        };
    }

    private static ReviewRecommendation DeriveRecommendation(
        double riskScore,
        double qualityScore,
        double authenticityScore,
        ReviewSentiment sentiment)
    {
        if (riskScore > 75)
            return ReviewRecommendation.Reject;

        if (authenticityScore < 35)
            return ReviewRecommendation.ManualReview;

        if (qualityScore >= 70 && sentiment is not (ReviewSentiment.Negative or ReviewSentiment.VeryNegative))
            return ReviewRecommendation.Publish;

        if (qualityScore >= 50)
            return ReviewRecommendation.Warn;

        return ReviewRecommendation.Improve;
    }

    private static ReviewSummaryDto? BuildSummary(ReviewAiResponse ai)
    {
        if (ai.Summary is { } rawSummary)
        {
            var shortSummary = string.IsNullOrWhiteSpace(rawSummary.ShortSummary)
                ? ai.FlatSummary ?? string.Empty
                : rawSummary.ShortSummary;

            return new ReviewSummaryDto(
                shortSummary,
                rawSummary.PositivePoints ?? [],
                rawSummary.NegativePoints ?? [],
                rawSummary.KeyTopics ?? []);
        }

        if (!string.IsNullOrWhiteSpace(ai.FlatSummary))
            return new ReviewSummaryDto(ai.FlatSummary, [], [], []);

        return null;
    }

    private static double DeriveToxicity(ReviewAiResponse ai, double riskScore)
    {
        if (ai.Categories.Count > 0)
        {
            var toxicCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Profanity", "Offense", "Insult", "ToxicLanguage", "Bullying", "Harassment",
                "Threat", "Violence", "HateSpeech", "Racism", "Sexism", "SexualContent",
                "Blackmail", "SelfHarm"
            };

            var maxToxicScore = ai.Categories
                .Where(c => toxicCategories.Contains(c.Category))
                .Select(c => Math.Clamp(c.Score ?? 0, 0, 1))
                .DefaultIfEmpty(0)
                .Max();

            if (maxToxicScore > 0)
                return Math.Round(maxToxicScore * 100);
        }

        return riskScore;
    }

    private static double DeriveRiskScore(ReviewAiResponse ai)
    {
        if (ParseEnum<RiskLevel>(ai.RiskLevel) is { } level)
        {
            return level switch
            {
                RiskLevel.Safe => 10,
                RiskLevel.Low => 30,
                RiskLevel.Medium => 50,
                RiskLevel.High => 70,
                _ => 90
            };
        }

        if (ai.Categories.Count > 0)
        {
            var maxScore = ai.Categories.Max(c => Math.Clamp(c.Score ?? 0, 0, 1));
            if (maxScore > 0)
                return maxScore * 100;
        }

        return 10;
    }

    private static double DeriveQualityScore(ReviewAiResponse ai, double riskScore)
    {
        if (ai.Categories.Any(c => string.Equals(c.Category, "FakeReview", StringComparison.OrdinalIgnoreCase)))
            return 10;

        return Math.Max(0, 100 - riskScore);
    }

    private static T? ParseEnum<T>(string? value) where T : struct, Enum
        => Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : null;

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

    private static ReviewAction MapAction(double score)
    {
        return score switch
        {
            > 90 => ReviewAction.Reject,
            > 75 => ReviewAction.Mask,
            > 50 => ReviewAction.Warn,
            _ => ReviewAction.Allow
        };
    }

    private static string BuildDefaultReason(ReviewAction action, double riskScore)
    {
        return action switch
        {
            ReviewAction.Reject => $"The review was rejected due to high risk (risk score {riskScore:0}).",
            ReviewAction.Mask => $"The review requires masking of sensitive data (risk score {riskScore:0}).",
            ReviewAction.Warn => $"The review contains potentially inappropriate material (risk score {riskScore:0}).",
            ReviewAction.ManualReview => "The review requires manual review.",
            _ => "The review appears safe."
        };
    }

    private static ModerationProviderException MapHttpFailure(HttpRequestException ex)
    {
        var kind = ex.StatusCode switch
        {
            HttpStatusCode.BadRequest => ModerationFailureKind.BadRequest,
            HttpStatusCode.Unauthorized => ModerationFailureKind.Unauthorized,
            HttpStatusCode.Forbidden => ModerationFailureKind.Forbidden,
            HttpStatusCode.RequestTimeout => ModerationFailureKind.Timeout,
            HttpStatusCode.TooManyRequests => ModerationFailureKind.Network,
            null => ModerationFailureKind.Network,
            _ when (int)ex.StatusCode >= 500 => ModerationFailureKind.Internal,
            _ => ModerationFailureKind.Network
        };

        var isTransient = kind is ModerationFailureKind.Timeout
            or ModerationFailureKind.Network
            or ModerationFailureKind.Internal;

        return new ModerationProviderException(
            kind,
            "The review moderation AI service rejected or failed the request.",
            isTransient,
            ex);
    }

    private PromptExecutionSettings CreateSettings()
    {
        return new PromptExecutionSettings
        {
            ModelId = _options.ModelId,
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = _options.Temperature,
                ["max_tokens"] = _options.MaxTokens
            }
        };
    }
}
