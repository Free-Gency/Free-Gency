using FreeGency.AI.ChatModeration.Enums;
using FreeGency.AI.ChatModeration.Models;

namespace FreeGency.AI.ChatModeration.Interfaces;

/// <summary>
/// Builds the system and user prompts used by the AI Chat Security moderation engine.
/// </summary>
public interface IChatSecurityPromptBuilder
{
    /// <summary>
    /// Builds the <see cref="ModerationPrompt"/> for the given chat context.
    /// </summary>
    /// <param name="context">The chat context containing the message, previous messages, roles, and identifiers.</param>
    /// <param name="contentType">The type of content being moderated.</param>
    /// <param name="conversationType">The type of conversation the message belongs to.</param>
    ModerationPrompt Build(
        ChatModerationContext context,
        ContentType contentType = ContentType.ChatMessage,
        ConversationType conversationType = ConversationType.PrivateChat);
}
