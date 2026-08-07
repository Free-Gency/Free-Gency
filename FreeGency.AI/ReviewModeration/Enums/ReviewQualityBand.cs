namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The quality band derived from the review quality score, following the product
/// business rules: 90+ Excellent, 70-89 Good, 50-69 Average, 30-49 Poor, below 30
/// Very Poor.
/// </summary>
public enum ReviewQualityBand
{
    /// <summary>Quality score 90 or above.</summary>
    Excellent = 0,

    /// <summary>Quality score 70 to 89.</summary>
    Good = 1,

    /// <summary>Quality score 50 to 69.</summary>
    Average = 2,

    /// <summary>Quality score 30 to 49.</summary>
    Poor = 3,

    /// <summary>Quality score below 30.</summary>
    VeryPoor = 4
}
