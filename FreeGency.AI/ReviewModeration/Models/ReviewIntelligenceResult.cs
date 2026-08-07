using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Models;

/// <summary>
/// The complete "review understanding" block produced for a review: sentiment,
/// quality, constructiveness, authenticity, rating consistency, length, writing
/// style, language, summary, strengths, weaknesses, tags, and the recommended
/// next step. It is produced twice for every request — by the AI provider and by
/// the deterministic <see cref="Contracts.IReviewIntelligenceAnalyzer"/> — and
/// merged by the review moderation service, so the understanding survives AI
/// outages. Internal carrier; never serialized directly.
/// </summary>
public sealed record ReviewIntelligenceResult(
    ReviewSentiment Sentiment,
    double SentimentConfidence,
    double QualityScore,
    ReviewQualityBand QualityBand,
    ReviewQualityBreakdownDto? QualityBreakdown,
    bool Constructive,
    double ConstructivenessScore,
    double AuthenticityScore,
    ReviewRatingConsistency RatingConsistency,
    ReviewLengthCategory LengthCategory,
    ReviewWritingStyle WritingStyle,
    ReviewLanguage Language,
    ReviewSummaryDto? Summary,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> Tags,
    ReviewRecommendation Recommendation)
{
    /// <summary>
    /// The neutral, uninformative intelligence used when no analysis is possible
    /// (empty text, invalid request). The service always merges this with the
    /// deterministic result, so its values are only meaningful as safe defaults.
    /// </summary>
    public static ReviewIntelligenceResult None { get; } = new(
        ReviewSentiment.Neutral,
        0,
        0,
        ReviewQualityBand.VeryPoor,
        null,
        false,
        0,
        0,
        ReviewRatingConsistency.Unknown,
        ReviewLengthCategory.Normal,
        ReviewWritingStyle.Casual,
        ReviewLanguage.Unknown,
        null,
        [],
        [],
        [],
        ReviewRecommendation.ManualReview);
}
