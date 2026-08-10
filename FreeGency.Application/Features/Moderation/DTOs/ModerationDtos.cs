using FreeGency.Domain.Enums;

namespace FreeGency.Application.Features.Moderation.DTOs;

public sealed class ContentModerationResult
{
    public ModerationAction Action { get; init; }
    public ModerationStatus Status { get; init; } = ModerationStatus.Visible;
    public string? SafeText { get; init; }
    public string? WarningMessage { get; init; }
    public Guid? CaseId { get; init; }
    public bool IsMuted { get; init; }
    public DateTime? MutedUntil { get; init; }
}

public sealed class ModerationCaseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public Guid SourceId { get; init; }
    public string Categories { get; init; } = string.Empty;
    public float Confidence { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? UserMessage { get; init; }
    public string? AdminSummary { get; init; }
    public string? AdminNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public sealed class ResolveModerationCaseRequest
{
    public string? AdminNote { get; set; }
}

public sealed class MyModerationStatusDto
{
    public bool HasViolations { get; init; }
    public int StrikeCount { get; init; }
    public int StrikeThreshold { get; init; }
    public int StrikeWindowDays { get; init; }
    public int StrikesRemainingUntilRestriction { get; init; }
    public bool IsRestricted { get; init; }
    public DateTime? RestrictedUntil { get; init; }
    public string RestrictionSummary { get; init; } = string.Empty;
    public IReadOnlyList<MyModerationStrikeDto> RecentStrikes { get; init; } = [];
}

public sealed class MyModerationStrikeDto
{
    public Guid Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string CategoryLabel { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string? UserMessage { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
