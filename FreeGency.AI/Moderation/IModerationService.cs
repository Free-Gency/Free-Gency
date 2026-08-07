using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation;

/// <summary>
/// The public facade over the reusable moderation core. It always returns a
/// <see cref="ModerationResult"/> and never throws, so consuming features can
/// map results to their own policies (allow, warn, reject, manual review).
/// </summary>
public interface IModerationService
{
    /// <summary>Moderates the given content and returns a structured verdict.</summary>
    Task<ModerationResult> ModerateAsync(ModerationRequest request, CancellationToken ct = default);

    /// <summary>Returns whether the given content is safe to publish.</summary>
    Task<bool> IsSafeAsync(string content, CancellationToken ct = default);

    /// <summary>Returns the risk score (0..100) of the given content.</summary>
    Task<double> CalculateRiskScoreAsync(ModerationRequest request, CancellationToken ct = default);
}
