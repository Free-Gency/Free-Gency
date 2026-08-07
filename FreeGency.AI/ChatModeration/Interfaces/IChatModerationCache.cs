using FreeGency.AI.ChatModeration.DTOs;

namespace FreeGency.AI.ChatModeration.Interfaces;

public interface IChatModerationCache
{
    Task<ModerationResult?> TryGetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, ModerationResult result, TimeSpan? expiration = null, CancellationToken ct = default);
    string BuildKey(ModerationRequest request);
}
