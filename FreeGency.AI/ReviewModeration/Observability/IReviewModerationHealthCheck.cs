namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Evaluates the health of the review moderation pipeline: AI provider status,
/// cache status, prompt loader status, and configuration status.
/// </summary>
public interface IReviewModerationHealthCheck
{
    /// <summary>Evaluates the current pipeline health synchronously.</summary>
    ReviewModerationHealth Evaluate();
}
