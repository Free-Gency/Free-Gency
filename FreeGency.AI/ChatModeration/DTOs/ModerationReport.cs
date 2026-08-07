namespace FreeGency.AI.ChatModeration.DTOs;

public sealed class ModerationReport
{
    public required int TotalCount { get; init; }
    public required int AllowedCount { get; init; }
    public required int FlaggedCount { get; init; }
    public required int RequireReviewCount { get; init; }
    public required int BlockedCount { get; init; }
    public required IReadOnlyList<ModerationResult> Results { get; init; }
}
