namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The recommended enforcement action for a moderated review. Review-scoped so
/// the review API never depends on moderation feature internals.
/// </summary>
public enum ReviewAction
{
    /// <summary>The review is safe and can be published.</summary>
    Allow = 0,

    /// <summary>The review is borderline; the author should be warned.</summary>
    Warn = 1,

    /// <summary>Sensitive parts of the review should be masked before publication.</summary>
    Mask = 2,

    /// <summary>The review must be blocked from publication.</summary>
    Reject = 3,

    /// <summary>The review should be escalated to a human moderator.</summary>
    ManualReview = 4
}
