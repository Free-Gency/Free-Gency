using FreeGency.AI.ReviewModeration.DTOs;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// The public entry point for review moderation. Implementations validate and
/// normalize the request, reuse the shared AI cache, delegate to the
/// <see cref="IReviewModerationProvider"/>, post-process the result, and return
/// a client-safe <see cref="ReviewModerationResponse"/>. Never throws to callers.
/// </summary>
public interface IReviewModerationService
{
    /// <summary>
    /// Moderates a review and returns a verdict. On invalid input or an
    /// unavailable AI service the result falls back to
    /// <see cref="Enums.ReviewAction.ManualReview"/>.
    /// </summary>
    Task<ReviewModerationResponse> ModerateAsync(ReviewModerationRequest request, CancellationToken ct = default);
}
