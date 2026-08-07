using FreeGency.AI.Moderation.Models;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Moderation.Prompts;

/// <summary>
/// The default prompt registry. Registers the general content moderation prompt
/// and exposes it under <see cref="ModerationSystemPrompt.DefaultPromptName"/>.
/// The registry is read-only after construction, so it is thread-safe.
/// </summary>
public sealed class ModerationPromptRegistry : IModerationPromptRegistry
{
    private readonly IReadOnlyDictionary<string, PromptDefinition> _prompts;
    private readonly IReadOnlyCollection<PromptDefinition> _allPrompts;
    private readonly string _currentName;

    public ModerationPromptRegistry(IOptions<ModerationOptions> options)
    {
        _currentName = string.IsNullOrWhiteSpace(options.Value.PromptName)
            ? ModerationSystemPrompt.DefaultPromptName
            : options.Value.PromptName;

        var version = new PromptVersion(
            string.IsNullOrWhiteSpace(options.Value.PromptVersion)
                ? PromptVersion.Current.Value
                : options.Value.PromptVersion);

        _prompts = new Dictionary<string, PromptDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [ModerationSystemPrompt.DefaultPromptName] = new PromptDefinition(
                ModerationSystemPrompt.DefaultPromptName,
                version,
                "General content moderation for any user-generated content.",
                "Chat messages, reviews, comments, project descriptions, proposal cover letters, profiles, and support tickets.",
                ModerationSystemPrompt.System)
        };

        _allPrompts = _prompts.Values.ToArray();
    }

    public IReadOnlyCollection<PromptDefinition> AllPrompts => _allPrompts;

    public PromptDefinition GetCurrentPrompt()
    {
        return _prompts.TryGetValue(_currentName, out var prompt)
            ? prompt
            : _prompts[ModerationSystemPrompt.DefaultPromptName];
    }

    public PromptDefinition GetPrompt(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_prompts.TryGetValue(name, out var prompt))
        {
            throw new KeyNotFoundException(
                $"No moderation prompt named '{name}' is registered. Available prompts: {string.Join(", ", _prompts.Keys)}.");
        }

        return prompt;
    }

    public bool TryGetPrompt(string name, out PromptDefinition? prompt) => _prompts.TryGetValue(name, out prompt);
}
