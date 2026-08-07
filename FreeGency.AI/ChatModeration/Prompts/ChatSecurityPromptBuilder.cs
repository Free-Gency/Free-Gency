using System.Text;
using FreeGency.AI.ChatModeration.Constants;
using FreeGency.AI.ChatModeration.Enums;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Models;

namespace FreeGency.AI.ChatModeration.Prompts;

/// <summary>
/// Builds the moderation prompt for the AI Chat Security engine using the
/// reusable <see cref="ChatSecurityPrompt.System"/> system prompt.
/// </summary>
public sealed class ChatSecurityPromptBuilder : IChatSecurityPromptBuilder
{
    public ModerationPrompt Build(
        ChatModerationContext context,
        ContentType contentType = ContentType.ChatMessage,
        ConversationType conversationType = ConversationType.PrivateChat)
    {
        var userPrompt = new StringBuilder();

        userPrompt.AppendLine("=== CONTENT TYPE ===");
        userPrompt.AppendLine(contentType.ToString());
        userPrompt.AppendLine();

        userPrompt.AppendLine("=== CONVERSATION TYPE ===");
        userPrompt.AppendLine(conversationType.ToString());
        userPrompt.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.ConversationId))
        {
            userPrompt.AppendLine("=== CONVERSATION ID ===");
            userPrompt.AppendLine(context.ConversationId);
            userPrompt.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(context.ProjectId))
        {
            userPrompt.AppendLine("=== PROJECT CONTEXT ===");
            userPrompt.AppendLine($"Project ID: {context.ProjectId}");
            userPrompt.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(context.SenderRole))
        {
            userPrompt.AppendLine("=== SENDER ROLE ===");
            userPrompt.AppendLine(context.SenderRole);
            userPrompt.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(context.ReceiverRole))
        {
            userPrompt.AppendLine("=== RECEIVER ROLE ===");
            userPrompt.AppendLine(context.ReceiverRole);
            userPrompt.AppendLine();
        }

        if (context.PreviousMessages.Count > 0)
        {
            userPrompt.AppendLine("=== PREVIOUS MESSAGES (oldest to newest) ===");
            for (var i = 0; i < context.PreviousMessages.Count; i++)
                userPrompt.AppendLine($"- {context.PreviousMessages[i]}");
            userPrompt.AppendLine();
        }

        userPrompt.AppendLine("=== MESSAGE TO MODERATE ===");
        userPrompt.Append(context.CurrentMessage);

        return new ModerationPrompt(ChatSecurityPrompt.System, userPrompt.ToString());
    }
}
