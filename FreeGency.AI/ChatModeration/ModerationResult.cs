using FreeGency.AI.ChatModeration.DTOs;

namespace FreeGency.AI.ChatModeration.Results;

/// <summary>
/// Wraps a typed moderation response together with the raw model output for traceability.
/// Lives in its own namespace to avoid a name collision with the existing
/// <see cref="ModerationResult"/> DTO in <c>FreeGency.AI.ChatModeration.DTOs</c>,
/// which is imported side by side with the Models namespace in existing services.
/// </summary>
/// <param name="ChatModerationResponse">The typed, validated moderation result.</param>
/// <param name="RawAIResponse">The raw model output, when available.</param>
public sealed record ModerationResult(ChatModerationResponse ChatModerationResponse, string? RawAIResponse);
