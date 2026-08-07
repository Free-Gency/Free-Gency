using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Monitoring;

/// <summary>
/// Base class for the structured, thread-safe loggers used by every AI module.
/// Wraps an <see cref="ILogger"/> and stamps every line with the audit fields
/// from the current <see cref="ICorrelationContextAccessor"/>: request id,
/// correlation id, and UTC timestamp. Never logs raw content; module loggers
/// are responsible for passing only hashes, lengths, counts, and safe enums.
/// </summary>
public abstract class ModerationStructuredLogger
{
    private readonly ILogger _logger;
    private readonly ICorrelationContextAccessor _correlation;
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastAlert = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes the logger with the sink and the correlation accessor used
    /// to stamp every event.
    /// </summary>
    protected ModerationStructuredLogger(ILogger logger, ICorrelationContextAccessor correlation)
    {
        _logger = logger;
        _correlation = correlation;
    }

    /// <summary>
    /// Writes a structured event with the given name, message, and fields. The
    /// event is stamped with the current request id, correlation id, and UTC
    /// timestamp from the correlation accessor.
    /// </summary>
    protected void Log(
        LogLevel level,
        string eventName,
        string message,
        params (string Key, object? Value)[] fields)
    {
        var context = _correlation.Current;
        var state = new List<KeyValuePair<string, object?>>(fields.Length + 4)
        {
            new("Event", eventName),
            new("RequestId", context?.RequestId ?? Guid.Empty),
            new("CorrelationId", context?.CorrelationId ?? Guid.Empty),
            new("TimestampUtc", context?.StartedAtUtc ?? DateTimeOffset.UtcNow)
        };

        foreach (var (key, value) in fields)
            state.Add(new KeyValuePair<string, object?>(key, value));

        _logger.Log(level, 0, new StructuredLogState(message, state), null, static (logState, _) => logState.ToString());
    }

    /// <summary>
    /// Writes an error-level structured event carrying the exception.
    /// </summary>
    protected void LogError(string eventName, string message, Exception exception)
    {
        var context = _correlation.Current;
        var state = new List<KeyValuePair<string, object?>>
        {
            new("Event", eventName),
            new("RequestId", context?.RequestId ?? Guid.Empty),
            new("CorrelationId", context?.CorrelationId ?? Guid.Empty),
            new("TimestampUtc", context?.StartedAtUtc ?? DateTimeOffset.UtcNow)
        };

        _logger.Log(LogLevel.Error, 0, new StructuredLogState(message, state), exception, static (logState, _) => logState.ToString());
    }

    /// <summary>
    /// Emits <paramref name="emit"/> at most once per <paramref name="cooldown"/>
    /// for the given key, so repeated performance alerts never flood the logs.
    /// </summary>
    protected void EmitCooldown(string key, TimeSpan cooldown, Action emit)
    {
        var now = DateTimeOffset.UtcNow;
        var last = _lastAlert.GetOrAdd(key, now);

        if (now - last < cooldown)
            return;

        if (_lastAlert.TryUpdate(key, now, last))
            emit();
    }
}
