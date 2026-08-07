using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// Deterministically detects spam signals in a review using only the review text
/// and, when available, the reviewer's previous reviews. Stateless and
/// thread-safe; reusable for any user-generated text.
/// </summary>
public interface IReviewSpamDetector
{
    /// <summary>
    /// Detects spam signals in <paramref name="text"/>. When
    /// <paramref name="previousReviews"/> is provided, the text is also compared
    /// against them to catch duplicate and copy-paste reviews.
    /// </summary>
    ReviewSpamResult Detect(string text, IReadOnlyList<ReviewModerationRequest>? previousReviews);
}
