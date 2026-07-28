namespace FreeGency.AI.DTOs;

public sealed class EmbeddingDto
{
    public string Id { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public float[] Vector { get; init; } = [];
    public IDictionary<string, string>? Metadata { get; init; }
}
