namespace FreeGency.AI.ChatModeration.Interfaces;

public interface IChatModerationMetrics
{
    void RecordRequest(string decision, double confidence, TimeSpan elapsed, bool fromCache);
    IReadOnlyDictionary<string, long> Snapshot();
}
