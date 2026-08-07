namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The detected writing style of a review.
/// </summary>
public enum ReviewWritingStyle
{
    /// <summary>The review is written in a professional register.</summary>
    Professional = 0,

    /// <summary>The review is written in a relaxed, everyday register.</summary>
    Casual = 1,

    /// <summary>The review uses hostile or confrontational language.</summary>
    Aggressive = 2,

    /// <summary>The review uses warm, encouraging language.</summary>
    Friendly = 3,

    /// <summary>The review uses a formal, measured register.</summary>
    Formal = 4,

    /// <summary>The review uses an informal, colloquial register.</summary>
    Informal = 5
}
