namespace FreeGency.AI.ReviewModeration.Models;

/// <summary>
/// A fully assembled review moderation prompt: the system instruction plus the
/// per-request user content. Never logged or returned to clients in raw form.
/// </summary>
/// <param name="SystemPrompt">The versioned system instruction.</param>
/// <param name="UserPrompt">The per-request context and review text.</param>
/// <param name="Version">The prompt template version.</param>
public sealed record ReviewPrompt(string SystemPrompt, string UserPrompt, string Version);
