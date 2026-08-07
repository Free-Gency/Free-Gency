using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation.Prompts;

/// <summary>
/// Builds the user part of a moderation prompt for a given request, selecting
/// the system instruction from the <see cref="IModerationPromptRegistry"/>.
/// </summary>
public interface IModerationPromptBuilder
{
    /// <summary>
    /// Builds a complete prompt for the given request. When <paramref name="promptName"/>
    /// is null, the currently configured prompt is used.
    /// </summary>
    ModerationPrompt Build(ModerationRequest request, string? promptName = null);
}
