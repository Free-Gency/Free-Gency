namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The dominant language of a review, as detected automatically. Used to route
/// the review to the right interpretation path.
/// </summary>
public enum ReviewLanguage
{
    /// <summary>The review is written in Arabic.</summary>
    Arabic = 0,

    /// <summary>The review is written in English.</summary>
    English = 1,

    /// <summary>The review mixes Arabic and Latin scripts or languages.</summary>
    Mixed = 2,

    /// <summary>
    /// The review is Arabic written with Latin letters and digits (Arabizi),
    /// for example "kosomk", "5awal", "ya 7ayawan".
    /// </summary>
    FrancoArabic = 3,

    /// <summary>The language could not be determined.</summary>
    Unknown = 4
}
