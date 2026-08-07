using System.ComponentModel.DataAnnotations;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.DTOs;

/// <summary>
/// Immutable result of moderating a chat message.
/// </summary>
public sealed record ChatModerationResponse
{
    /// <summary>Gets a value indicating whether the message is safe to publish.</summary>
    public required bool IsSafe { get; init; }

    /// <summary>Gets the overall risk score on a 0..1 scale, higher means riskier.</summary>
    [Range(0.0, 1.0)]
    public required double RiskScore { get; init; }

    /// <summary>Gets the model confidence on a 0..1 scale.</summary>
    [Range(0.0, 1.0)]
    public required double Confidence { get; init; }

    /// <summary>Gets the overall risk level.</summary>
    public required RiskLevel RiskLevel { get; init; }

    /// <summary>Gets the action recommended for the message.</summary>
    public required ModerationAction Action { get; init; }

    /// <summary>Gets a short human-readable explanation of the decision.</summary>
    public string? Reason { get; init; }

    /// <summary>Gets a concise natural-language summary of the analysis.</summary>
    public string? Summary { get; init; }

    /// <summary>Gets the per-category risk scores detected in the message.</summary>
    public IReadOnlyList<CategoryScoreDto> Categories { get; init; } = [];

    /// <summary>Gets the entities (phones, emails, URLs, ...) detected in the message.</summary>
    public IReadOnlyList<DetectedEntityDto> DetectedEntities { get; init; } = [];

    /// <summary>Gets the ISO language codes detected in the message.</summary>
    public IReadOnlyList<string> DetectedLanguages { get; init; } = [];

    /// <summary>Gets the sensitive or risky keywords matched in the message.</summary>
    public IReadOnlyList<string> MatchedKeywords { get; init; } = [];

    /// <summary>Gets the message with sensitive data masked, when applicable.</summary>
    public string? MaskedMessage { get; init; }

    /// <summary>Gets the total processing time in milliseconds.</summary>
    public long ProcessingTimeMs { get; init; }

    /// <summary>Gets the name of the model that produced the analysis, when known.</summary>
    public string? ModelName { get; init; }

    /// <summary>Gets a value indicating whether the result was served from cache.</summary>
    public bool UsedCache { get; init; }

    /// <summary>Gets the UTC timestamp when the moderation completed.</summary>
    public required DateTimeOffset Timestamp { get; init; }
}
