namespace FreeGency.AI.ChatModeration.Models;

public sealed class ModerationAiResult
{
    public string? Decision { get; init; }
    public string? Severity { get; init; }
    public double Confidence { get; init; }
    public string? Summary { get; init; }
    public List<ModerationCategory> Categories { get; init; } = [];
    public string? SanitizedContent { get; init; }
}

public sealed class ModerationCategory
{
    public string Name { get; init; } = string.Empty;
    public string? Severity { get; init; }
    public double Score { get; init; }
}
