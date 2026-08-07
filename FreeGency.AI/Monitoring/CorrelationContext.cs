namespace FreeGency.AI.Monitoring;

/// <summary>
/// The per-request correlation primitives shared by every AI module. Stored in
/// an <see cref="AsyncLocal{T}"/>-backed accessor so the AI provider, cache, and
/// logger can stamp their events with the same request id and correlation id
/// without threading the context through every call.
/// </summary>
/// <param name="RequestId">The unique id of the current request.</param>
/// <param name="CorrelationId">The correlation id propagated to every downstream call.</param>
/// <param name="StartedAtUtc">The moment the request started (UTC), used as the timestamp for all stamped events.</param>
public sealed record CorrelationContext(
    Guid RequestId,
    Guid CorrelationId,
    DateTimeOffset StartedAtUtc);
