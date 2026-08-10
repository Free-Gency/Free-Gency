using FreeGency.Domain.Enums;

namespace FreeGency.AI.Moderation;

public sealed class ModerationDecision
{
    public IReadOnlyList<ModerationCategory> Categories { get; init; } = [ModerationCategory.Clean];
    public float Confidence { get; init; }
    public ModerationAction Action { get; init; } = ModerationAction.Allow;
    public string UserMessage { get; init; } = string.Empty;
    public string AdminSummary { get; init; } = string.Empty;
    public string? RedactedText { get; init; }
    public bool UsedLlm { get; init; }
}

public sealed class ModerationRequest
{
    public string Content { get; init; } = string.Empty;
    public string Surface { get; init; } = "chat"; // chat | review
}
