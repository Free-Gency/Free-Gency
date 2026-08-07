namespace FreeGency.AI.ChatModeration.Models;

/// <summary>
/// Full context about a chat exchange needed to moderate a single message.
/// </summary>
public sealed record ChatModerationContext
{
    /// <summary>Gets the message currently being moderated.</summary>
    public required string CurrentMessage { get; init; }

    /// <summary>Gets the most recent prior messages, oldest to newest.</summary>
    public IReadOnlyList<string> PreviousMessages { get; init; } = [];

    /// <summary>Gets the conversation identifier, when known.</summary>
    public string? ConversationId { get; init; }

    /// <summary>Gets the project identifier, when known.</summary>
    public string? ProjectId { get; init; }

    /// <summary>Gets the sender identifier, when known.</summary>
    public string? SenderId { get; init; }

    /// <summary>Gets the receiver identifier, when known.</summary>
    public string? ReceiverId { get; init; }

    /// <summary>Gets the sender role, when known.</summary>
    public string? SenderRole { get; init; }

    /// <summary>Gets the receiver role, when known.</summary>
    public string? ReceiverRole { get; init; }
}
