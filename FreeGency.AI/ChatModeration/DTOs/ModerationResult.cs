using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.DTOs;

public sealed class ModerationResult
{
    public required string ContentHash { get; init; }
    public required ModerationDecision Decision { get; init; }
    public required ModerationSeverity Severity { get; init; }
    public double Confidence { get; init; }
    public required bool IsApproved { get; init; }
    public required bool FromCache { get; init; }
    public required IReadOnlyList<ModerationCategoryResult> Categories { get; init; }
    public string? Summary { get; init; }
    public string? SanitizedContent { get; init; }
    public string? ModelUsed { get; init; }
    public DateTimeOffset ProcessedAt { get; init; } = DateTimeOffset.UtcNow;
    public TimeSpan Elapsed { get; init; }
}

public sealed class ModerationCategoryResult
{
    public required string Name { get; init; }
    public required ModerationSeverity Severity { get; init; }
    public double Score { get; init; }
}
