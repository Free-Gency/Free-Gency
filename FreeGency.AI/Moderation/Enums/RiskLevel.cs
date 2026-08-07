namespace FreeGency.AI.Moderation.Enums;

/// <summary>
/// Classifies the overall risk of a piece of moderated content, from safe to
/// critical. This enum is feature-agnostic and independent from the legacy
/// <c>FreeGency.AI.ChatModeration.Enums.RiskLevel</c>.
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
