namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// Classifies the overall risk of a review, from safe to critical. Review-scoped
/// so the review API never depends on moderation feature internals.
/// </summary>
public enum RiskLevel
{
    /// <summary>No risk detected.</summary>
    Safe = 0,

    /// <summary>Negligible risk.</summary>
    Low = 1,

    /// <summary>Moderate risk requiring attention.</summary>
    Medium = 2,

    /// <summary>Elevated risk.</summary>
    High = 3,

    /// <summary>Severe risk requiring immediate action.</summary>
    Critical = 4
}
