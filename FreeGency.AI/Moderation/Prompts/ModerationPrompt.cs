namespace FreeGency.AI.Moderation.Prompts;

/// <summary>
/// A fully assembled moderation prompt: the system instruction plus the
/// per-request user content. Never logged or returned to clients in raw form.
/// </summary>
public sealed record ModerationPrompt(string SystemPrompt, string UserPrompt);
