namespace FreeGency.AI.Moderation;

public interface IModerationAgent
{
    Task<ModerationDecision> ModerateAsync(ModerationRequest request, CancellationToken ct = default);
}
