using Microsoft.Extensions.Options;

namespace FreeGency.AI.ChatModeration.RateLimiting;

/// <summary>
/// Fixed-window in-memory rate limiter (one window per calendar minute) used by
/// the chat moderation API. It enforces separate per-user and per-conversation
/// limits and returns the caller a <see cref="RateLimitDecision"/>.
///
/// Thread-safe: all state transitions happen under a single gate. Old windows
/// are lazily replaced on access and periodically swept to bound memory growth.
/// </summary>
public sealed class ChatModerationRateLimiter
{
    private readonly object _gate = new();
    private readonly ChatModerationRateLimitOptions _options;
    private readonly Dictionary<string, WindowEntry> _windows = new(StringComparer.Ordinal);

    public ChatModerationRateLimiter(IOptions<ChatModerationRateLimitOptions> options)
        => _options = options.Value;

    /// <summary>
    /// Checks whether a request for the given user and conversation is within
    /// the configured per-minute limits. Both keys are enforced independently.
    /// </summary>
    /// <param name="userKey">Rate-limit key identifying the authenticated user.</param>
    /// <param name="conversationKey">Rate-limit key identifying the conversation.</param>
    /// <param name="now">Current time; defaults to <see cref="DateTimeOffset.UtcNow"/>.</param>
    public RateLimitDecision Check(string userKey, string conversationKey, DateTimeOffset? now = null)
    {
        var current = now ?? DateTimeOffset.UtcNow;

        lock (_gate)
        {
            Sweep(current);

            if (!TryConsume(userKey, _options.MaxRequestsPerUserPerMinute, current, out var userRetryAfter))
                return RateLimitDecision.Rejected(userRetryAfter);

            if (!TryConsume(conversationKey, _options.MaxRequestsPerConversationPerMinute, current, out var conversationRetryAfter))
                return RateLimitDecision.Rejected(conversationRetryAfter);

            return RateLimitDecision.Allowed();
        }
    }

    private bool TryConsume(string key, int limit, DateTimeOffset now, out TimeSpan retryAfter)
    {
        var windowId = GetWindowId(now);

        retryAfter = TimeSpan.Zero;

        if (!_windows.TryGetValue(key, out var entry) || entry.WindowId != windowId)
        {
            _windows[key] = new WindowEntry(windowId, 1);
            return true;
        }

        if (entry.Count >= limit)
        {
            retryAfter = GetRemaining(now, windowId);
            return false;
        }

        _windows[key] = new WindowEntry(windowId, entry.Count + 1);
        return true;
    }

    private void Sweep(DateTimeOffset now)
    {
        if (_windows.Count < _options.MaxTrackedKeys)
            return;

        var currentWindowId = GetWindowId(now);
        foreach (var key in _windows.Keys)
        {
            if (_windows[key].WindowId != currentWindowId)
                _windows.Remove(key);
        }
    }

    private static long GetWindowId(DateTimeOffset now)
        => now.ToUnixTimeSeconds() / 60L;

    private static TimeSpan GetRemaining(DateTimeOffset now, long windowId)
    {
        var windowStart = DateTimeOffset.FromUnixTimeSeconds(windowId * 60L);
        var nextWindowStart = windowStart.AddMinutes(1);
        var remaining = nextWindowStart - now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.FromSeconds(1);
    }

    private sealed record WindowEntry(long WindowId, int Count);
}
