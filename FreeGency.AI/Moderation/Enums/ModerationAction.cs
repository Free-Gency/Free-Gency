namespace FreeGency.AI.Moderation.Enums;

/// <summary>
/// The recommended enforcement action for a piece of moderated content.
/// Feature-agnostic: any platform feature (chat, reviews, comments, project
/// descriptions, proposals, profiles, support tickets) maps this action to its
/// own policy without hardcoding "Chat" anywhere in the moderation core.
/// </summary>
public enum ModerationAction
{
    /// <summary>The content is safe and can be published or delivered.</summary>
    Allow = 0,

    /// <summary>The content is borderline; the author should be warned.</summary>
    Warn = 1,

    /// <summary>Sensitive content should be masked before publication.</summary>
    Mask = 2,

    /// <summary>The content must be blocked from publication.</summary>
    Reject = 3,

    /// <summary>The content should be escalated to a human moderator.</summary>
    ManualReview = 4,

    /// <summary>The author should be temporarily restricted.</summary>
    TemporaryMute = 5,

    /// <summary>The author should be permanently restricted.</summary>
    PermanentBan = 6
}
