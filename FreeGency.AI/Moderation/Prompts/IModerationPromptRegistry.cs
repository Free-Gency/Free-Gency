namespace FreeGency.AI.Moderation.Prompts;

/// <summary>
/// Provides versioned moderation prompt templates by name. Implementations are
/// immutable after construction so concurrent readers are always safe.
/// </summary>
public interface IModerationPromptRegistry
{
    /// <summary>Gets all registered prompt definitions, keyed by name.</summary>
    IReadOnlyCollection<PromptDefinition> AllPrompts { get; }

    /// <summary>
    /// Gets the currently configured prompt (see
    /// <c>FreeGency.AI.Moderation.ModerationOptions.PromptName</c>). Falls back to
    /// the default general-content prompt when the configured name is unknown.
    /// </summary>
    PromptDefinition GetCurrentPrompt();

    /// <summary>Gets the prompt with the given name. Throws when it does not exist.</summary>
    PromptDefinition GetPrompt(string name);

    /// <summary>Attempts to get the prompt with the given name.</summary>
    bool TryGetPrompt(string name, out PromptDefinition? prompt);
}
