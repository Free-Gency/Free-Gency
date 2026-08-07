using FreeGency.AI.ChatModeration.DTOs;

namespace FreeGency.AI.ChatModeration.Interfaces;

public interface IChatModerationService
{
    Task<ModerationResult> ModerateAsync(ModerationRequest request, CancellationToken ct = default);
    Task<ModerationReport> ModerateBatchAsync(IEnumerable<ModerationRequest> requests, CancellationToken ct = default);
}
