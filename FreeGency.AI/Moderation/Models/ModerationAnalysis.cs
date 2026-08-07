using FreeGency.AI.Moderation.Enums;

namespace FreeGency.AI.Moderation.Models;

/// <summary>
/// The structured verdict produced by the moderation engine. <see cref="RiskScore"/>
/// is an integer from 0 to 100 and <see cref="Confidence"/> is a number from 0 to 1.
/// </summary>
public sealed record ModerationAnalysis(
    bool IsSafe,
    double RiskScore,
    double Confidence,
    RiskLevel RiskLevel,
    ModerationAction Action,
    string? Reason,
    IReadOnlyList<ModerationCategoryScore> Categories,
    string PromptVersion,
    string? ModelName);
