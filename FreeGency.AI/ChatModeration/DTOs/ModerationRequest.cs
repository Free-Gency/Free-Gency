using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.DTOs;

public sealed class ModerationRequest
{
    public required string Content { get; init; }
    public ContentType ContentType { get; init; } = ContentType.ChatMessage;
    public string? AuthorId { get; init; }
    public string? EntityId { get; init; }
    public bool BypassCache { get; init; }
    public IDictionary<string, string>? Context { get; init; }
}
