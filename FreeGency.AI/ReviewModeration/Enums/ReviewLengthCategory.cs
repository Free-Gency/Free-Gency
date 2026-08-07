namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The length classification of a review based on its text length.
/// </summary>
public enum ReviewLengthCategory
{
    /// <summary>The review is very short (a few words).</summary>
    VeryShort = 0,

    /// <summary>The review is short.</summary>
    Short = 1,

    /// <summary>The review is of typical length.</summary>
    Normal = 2,

    /// <summary>The review is detailed.</summary>
    Detailed = 3,

    /// <summary>The review is very detailed.</summary>
    VeryDetailed = 4
}
