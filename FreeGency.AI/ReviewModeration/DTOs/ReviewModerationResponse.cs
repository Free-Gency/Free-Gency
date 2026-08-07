using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// The final result of moderating a review. <see cref="Approved"/> is derived
/// from the recommended <see cref="Action"/>: the review is approved when the
/// action is <see cref="ReviewAction.Allow"/>, <see cref="ReviewAction.Warn"/>, or
/// <see cref="ReviewAction.Mask"/>. Never throws; when the AI service fails the
/// action falls back to <see cref="ReviewAction.ManualReview"/>.
/// </summary>
public sealed record ReviewModerationResponse
{
    /// <summary>Gets a value indicating whether the review is approved for publication.</summary>
    public bool Approved { get; init; }

    /// <summary>Gets the overall risk score on a 0..100 scale.</summary>
    public double RiskScore { get; init; }

    /// <summary>Gets the confidence of the verdict on a 0..1 scale.</summary>
    public double Confidence { get; init; }

    /// <summary>Gets the overall risk level.</summary>
    public RiskLevel RiskLevel { get; init; }

    /// <summary>Gets the recommended enforcement action.</summary>
    public ReviewAction Action { get; init; }

    /// <summary>Gets the detected sentiment of the review.</summary>
    public ReviewSentiment Sentiment { get; init; }

    /// <summary>Gets the confidence of the sentiment judgment on a 0..1 scale.</summary>
    public double SentimentConfidence { get; init; }

    /// <summary>Gets the review quality score on a 0..100 scale.</summary>
    public double QualityScore { get; init; }

    /// <summary>Gets the quality band derived from <see cref="QualityScore"/>.</summary>
    public ReviewQualityBand QualityBand { get; init; }

    /// <summary>Gets the per-criterion quality assessment, when available.</summary>
    public ReviewQualityBreakdownDto? QualityBreakdown { get; init; }

    /// <summary>Gets a value indicating whether the review provides useful, actionable feedback.</summary>
    public bool Constructive { get; init; }

    /// <summary>Gets the constructiveness estimate on a 0..100 scale.</summary>
    public double ConstructivenessScore { get; init; }

    /// <summary>Gets the estimated authenticity of the review on a 0..100 scale.</summary>
    public double AuthenticityScore { get; init; }

    /// <summary>Gets whether the star rating agrees with the tone of the text.</summary>
    public ReviewRatingConsistency RatingConsistency { get; init; }

    /// <summary>Gets the length classification of the review.</summary>
    public ReviewLengthCategory LengthCategory { get; init; }

    /// <summary>Gets the detected writing style of the review.</summary>
    public ReviewWritingStyle WritingStyle { get; init; }

    /// <summary>Gets the automatically detected language of the review.</summary>
    public ReviewLanguage Language { get; init; }

    /// <summary>Gets the toxicity score on a 0..100 scale (0 is not toxic at all).</summary>
    public double ToxicityScore { get; init; }

    /// <summary>Gets the generated summary of the review, when available.</summary>
    public ReviewSummaryDto? Summary { get; init; }

    /// <summary>Gets a short professional reason for the verdict.</summary>
    public string? Reason { get; init; }

    /// <summary>Gets the detected policy violation categories and their scores.</summary>
    public IReadOnlyList<ReviewCategoryDto> DetectedCategories { get; init; } = [];

    /// <summary>Gets the typed security categories the review belongs to.</summary>
    public IReadOnlyList<ReviewSecurityCategory> SecurityCategories { get; init; } = [];

    /// <summary>Gets the deterministic spam signals that were triggered, if any.</summary>
    public IReadOnlyList<string> SpamSignals { get; init; } = [];

    /// <summary>Gets the detected entities (people, companies, contacts, PII).</summary>
    public IReadOnlyList<ReviewEntityDto> DetectedEntities { get; init; } = [];

    /// <summary>Gets the suggested tags extracted from the review.</summary>
    public IReadOnlyList<string> SuggestedTags { get; init; } = [];

    /// <summary>Gets the strengths the review praises, when any.</summary>
    public IReadOnlyList<string> Strengths { get; init; } = [];

    /// <summary>Gets the weaknesses the review criticizes, when any.</summary>
    public IReadOnlyList<string> Weaknesses { get; init; } = [];

    /// <summary>Gets the recommended next step for the review as content.</summary>
    public ReviewRecommendation Recommendation { get; init; }

    /// <summary>Gets the detected topic keywords extracted from the review.</summary>
    public IReadOnlyList<string> DetectedKeywords { get; init; } = [];

    /// <summary>Gets suggested improvements for the review, when available.</summary>
    public IReadOnlyList<ReviewSuggestionDto> Suggestions { get; init; } = [];

    /// <summary>Gets the review text with sensitive values masked, or null when nothing was masked.</summary>
    public string? MaskedReview { get; init; }

    /// <summary>Gets the review prompt template version used to produce this result.</summary>
    public string PromptVersion { get; init; } = string.Empty;

    /// <summary>Gets the AI model that produced this result.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets the total processing time in milliseconds.</summary>
    public long ProcessingTime { get; init; }

    /// <summary>Gets a value indicating whether the result was served from cache.</summary>
    public bool FromCache { get; init; }
}
