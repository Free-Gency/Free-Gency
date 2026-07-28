namespace FreeGency.AI.DTOs;

public sealed class AIRequest
{
    public required string Prompt { get; init; }
    public string? SystemPrompt { get; init; }
    public IDictionary<string, string>? Context { get; init; }
    public AIRequestType Type { get; init; } = AIRequestType.General;
    public int? MaxTokens { get; init; }
    public double? Temperature { get; init; }
}

public enum AIRequestType
{
    General,
    Analysis,
    Generation,
    Classification,
    Summarization,
    Embedding
}
