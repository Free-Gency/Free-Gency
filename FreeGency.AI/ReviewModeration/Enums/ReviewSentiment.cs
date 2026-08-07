namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The overall sentiment of a review, from very positive to very negative.
/// </summary>
public enum ReviewSentiment
{
    /// <summary>The review is very positive.</summary>
    VeryPositive = 0,

    /// <summary>The review is positive.</summary>
    Positive = 1,

    /// <summary>The review is neutral.</summary>
    Neutral = 2,

    /// <summary>The review is negative.</summary>
    Negative = 3,

    /// <summary>The review is very negative.</summary>
    VeryNegative = 4
}
