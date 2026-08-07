using System.Text;
using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation.Prompts;

/// <summary>
/// Builds the per-request user prompt from the registry's system instruction,
/// the content kind, optional surrounding context, and optional metadata.
/// </summary>
public sealed class ModerationPromptBuilder : IModerationPromptBuilder
{
    private readonly IModerationPromptRegistry _registry;

    public ModerationPromptBuilder(IModerationPromptRegistry registry)
    {
        _registry = registry;
    }

    public ModerationPrompt Build(ModerationRequest request, string? promptName = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        var definition = string.IsNullOrWhiteSpace(promptName)
            ? _registry.GetCurrentPrompt()
            : _registry.GetPrompt(promptName);

        var userPrompt = new StringBuilder();

        userPrompt.AppendLine("=== CONTENT KIND ===");
        userPrompt.AppendLine(string.IsNullOrWhiteSpace(request.ContentKind) ? "UserGeneratedContent" : request.ContentKind);
        userPrompt.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            userPrompt.AppendLine("=== SURROUNDING CONTEXT ===");
            userPrompt.AppendLine(request.Context);
            userPrompt.AppendLine();
        }

        if (request.Metadata is { Count: > 0 })
        {
            userPrompt.AppendLine("=== ADDITIONAL CONTEXT ===");
            foreach (var (key, value) in request.Metadata)
                userPrompt.AppendLine($"{key}: {value}");
            userPrompt.AppendLine();
        }

        userPrompt.AppendLine("=== CONTENT TO MODERATE ===");
        userPrompt.Append(request.Content);

        return new ModerationPrompt(definition.SystemPrompt, userPrompt.ToString());
    }
}
