namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// Whether the star rating matches the tone of the review text. A 5-star review
/// that says "this was terrible" and a 1-star review that says "excellent work"
/// are both inconsistent and therefore suspicious.
/// </summary>
public enum ReviewRatingConsistency
{
    /// <summary>The rating agrees with the tone of the text.</summary>
    Consistent = 0,

    /// <summary>The rating disagrees with the tone of the text.</summary>
    Inconsistent = 1,

    /// <summary>Consistency could not be determined (for example no rating).</summary>
    Unknown = 2
}
