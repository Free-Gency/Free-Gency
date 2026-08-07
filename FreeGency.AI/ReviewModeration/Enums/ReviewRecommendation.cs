namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The recommended next step for the review, independent of the enforcement
/// <see cref="ReviewAction"/>. It advises how the review itself should be treated
/// as content: publish it, warn the author, ask for improvement, send it to a
/// human moderator, or reject it entirely.
/// </summary>
public enum ReviewRecommendation
{
    /// <summary>The review is useful and safe to publish.</summary>
    Publish = 0,

    /// <summary>The review is publishable but warrants a warning to the author.</summary>
    Warn = 1,

    /// <summary>The review should be improved (too short, vague, or low quality).</summary>
    Improve = 2,

    /// <summary>The review should be escalated to a human moderator.</summary>
    ManualReview = 3,

    /// <summary>The review should be rejected (spam, duplicate, or fabricated).</summary>
    Reject = 4
}
