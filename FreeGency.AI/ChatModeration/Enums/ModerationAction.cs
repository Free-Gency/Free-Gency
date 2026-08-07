namespace FreeGency.AI.ChatModeration.Enums;

/// <summary>
/// The enforcement action recommended for a moderated message.
/// </summary>
public enum ModerationAction
{
    /// <summary>The message is safe and can be delivered.</summary>
    Allow = 0,

    /// <summary>The message is borderline; the sender should be warned.</summary>
    Warn = 1,

    /// <summary>Sensitive content should be masked before delivery.</summary>
    Mask = 2,

    /// <summary>The message must be blocked from delivery.</summary>
    Reject = 3,

    /// <summary>The message should be escalated to a human moderator.</summary>
    ManualReview = 4,

    /// <summary>The sender should be temporarily muted.</summary>
    TemporaryMute = 5,

    /// <summary>The sender should be permanently banned.</summary>
    PermanentBan = 6
}
