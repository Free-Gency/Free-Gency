using FreeGency.AI.Moderation.Reliability;
using FreeGency.AI.ReviewModeration.DTOs;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// The review moderation provider abstraction. Implementations only call the
/// existing AI layer (<c>IChatCompletionService</c>) and map the model reply into
/// a <see cref="ReviewAnalysisResult"/>. They never contain business rules or
/// validation. On failure, implementations throw
/// <see cref="ModerationProviderException"/>.
/// </summary>
public interface IReviewModerationProvider
{
    /// <summary>
    /// Gets the current circuit breaker state, used by the health and metrics
    /// surfaces to report whether the AI call path is currently short-circuited.
    /// </summary>
    CircuitBreakerState CircuitState { get; }

    /// <summary>
    /// Analyses a review and returns the structured result.
    /// </summary>
    Task<ReviewAnalysisResult> AnalyzeAsync(ReviewModerationRequest request, CancellationToken ct = default);
}
