using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// Produces the deterministic review understanding (sentiment, quality,
/// constructiveness, authenticity, rating consistency, length, writing style,
/// language, summary, strengths, weaknesses, tags, and recommendation) from the
/// review text alone. It never calls the AI layer, so it keeps working when the
/// AI service is down. Stateless and thread-safe.
/// </summary>
public interface IReviewIntelligenceAnalyzer
{
    /// <summary>
    /// Analyzes <paramref name="text"/> deterministically, using the optional
    /// <paramref name="rating"/> for the rating-consistency check and the optional
    /// <paramref name="previousReviews"/> to detect duplicate/fabricated content.
    /// </summary>
    ReviewIntelligenceResult Analyze(
        string text,
        int? rating,
        IReadOnlyList<ReviewModerationRequest>? previousReviews);
}
