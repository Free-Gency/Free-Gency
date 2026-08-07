namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The deterministic spam signals the spam detector can produce. A single review
/// may trigger several signals; <see cref="None"/> is never emitted.
/// </summary>
public enum ReviewSpamSignal
{
    /// <summary>No spam signal detected.</summary>
    None = 0,

    /// <summary>The review text is identical or nearly identical to a previous review.</summary>
    DuplicateReview = 1,

    /// <summary>The review strongly resembles a previous review by the same reviewer.</summary>
    CopyPaste = 2,

    /// <summary>The review is dominated by an excessive number of emojis.</summary>
    EmojiSpam = 3,

    /// <summary>The review contains long runs of a single repeated character.</summary>
    CharacterSpam = 4,

    /// <summary>The review text is mostly random or low-diversity characters.</summary>
    RandomText = 5,

    /// <summary>A single word is repeated excessively across the review.</summary>
    RepeatedWords = 6,

    /// <summary>The review carries no meaningful content.</summary>
    MeaninglessText = 7,

    /// <summary>The review is extremely short.</summary>
    VeryShort = 8
}
