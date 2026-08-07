namespace FreeGency.AI.ChatModeration.Interfaces;

public interface IChatModerationLogger
{
    void LogRequestStarted(string key);
    void LogRequestCompleted(string key, string decision, TimeSpan elapsed);
    void LogCacheHit(string key);
    void LogCacheMiss(string key);
    void LogParseFailure(string key);
    void LogError(string key, Exception exception);
}
