using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// Combines the deterministic security components (content masking and spam
/// detection) into a single <see cref="ReviewSecurityResult"/> for a review.
/// Never calls the AI layer, so it keeps working when the AI service is down.
/// Stateless and thread-safe.
/// </summary>
public interface IReviewSecurityAnalyzer
{
    /// <summary>
    /// Analyzes <paramref name="text"/> deterministically, optionally comparing it
    /// against the reviewer's <paramref name="previousReviews"/> for duplicates.
    /// </summary>
    ReviewSecurityResult Analyze(string text, IReadOnlyList<ReviewModerationRequest>? previousReviews);
}
