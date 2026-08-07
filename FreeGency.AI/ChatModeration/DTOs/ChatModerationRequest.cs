using System.ComponentModel.DataAnnotations;
using FreeGency.AI.ChatModeration.Constants;

namespace FreeGency.AI.ChatModeration.DTOs;

/// <summary>
/// Immutable request describing a single chat message to be moderated.
/// </summary>
public sealed record ChatModerationRequest
{
    /// <summary>Gets the chat message text to moderate.</summary>
    [StringLength(ChatModerationConstants.DefaultMaxContentLength, MinimumLength = 1)]
    public required string Message { get; init; }

    /// <summary>Gets the conversation identifier this message belongs to, if any.</summary>
    public string? ConversationId { get; init; }

    /// <summary>Gets the project identifier the conversation is associated with, if any.</summary>
    public string? ProjectId { get; init; }

    /// <summary>Gets the identifier of the user who sent the message, if known.</summary>
    public string? SenderId { get; init; }

    /// <summary>Gets the identifier of the user who will receive the message, if known.</summary>
    public string? ReceiverId { get; init; }

    /// <summary>Gets the sender role (e.g. "Client", "Freelancer", "Guest"), if known.</summary>
    public string? SenderRole { get; init; }

    /// <summary>Gets the receiver role (e.g. "Client", "Freelancer"), if known.</summary>
    public string? ReceiverRole { get; init; }

    /// <summary>Gets an optional ISO language code hint (e.g. "ar", "en").</summary>
    public string? Language { get; init; }

    /// <summary>Gets the most recent prior messages in the conversation, oldest to newest.</summary>
    public IReadOnlyList<string> PreviousMessages { get; init; } = [];

    /// <summary>Gets optional arbitrary metadata supplied by the caller.</summary>
    public IDictionary<string, string>? Metadata { get; init; }
}
