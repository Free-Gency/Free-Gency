using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation.Prompts;

/// <summary>
/// A registered moderation prompt template. Every prompt carries a name, a
/// version, a description, and its intended use case so operations can see what
/// is active, why it exists, and where it is applied.
/// </summary>
public sealed record PromptDefinition(
    string Name,
    PromptVersion Version,
    string Description,
    string UseCase,
    string SystemPrompt);
