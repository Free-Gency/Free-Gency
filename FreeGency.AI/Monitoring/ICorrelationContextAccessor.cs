namespace FreeGency.AI.Monitoring;

/// <summary>
/// Grants ambient access to the current <see cref="CorrelationContext"/>. The
/// context flows with the async execution context, so every component inside a
/// request (provider, cache, logger, metrics) can read the same request id and
/// correlation id without receiving it through its call chain. Safe to share
/// across concurrent requests; the value is per-execution-context, never static.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>Gets the current correlation context, or null outside a moderated request.</summary>
    CorrelationContext? Current { get; }

    /// <summary>Sets the correlation context for the current execution flow.</summary>
    void Set(CorrelationContext context);

    /// <summary>Clears the correlation context for the current execution flow.</summary>
    void Clear();
}
