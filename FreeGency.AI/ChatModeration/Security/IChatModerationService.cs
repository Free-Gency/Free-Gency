using FreeGency.AI.ChatModeration.DTOs;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Moderation service for the AI Chat Security feature. Lives in the
/// <c>Security</c> namespace to stay separate from the legacy moderation pipeline.
/// </summary>
public interface IChatModerationService
{
    /// <summary>Moderates a chat message and returns a structured verdict.</summary>
    Task<ChatModerationResponse> ModerateAsync(ChatModerationRequest request, CancellationToken ct = default);

    /// <summary>Returns whether the given message is safe to publish.</summary>
    Task<bool> IsSafeAsync(string message, CancellationToken ct = default);

    /// <summary>Returns the risk score (0..100) of the given message.</summary>
    Task<double> CalculateRiskScoreAsync(ChatModerationRequest request, CancellationToken ct = default);
}
