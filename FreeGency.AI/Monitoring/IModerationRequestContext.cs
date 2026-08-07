namespace FreeGency.AI.Monitoring;

/// <summary>
/// The common shape of every moderation request context across the AI modules
/// (review, chat, comment, portfolio, profile, project, proposal ranking).
/// Implementations add module-specific, non-sensitive fields (hashes, lengths,
/// identifiers, hints) on top of these three audit primitives, which are the
/// fields every structured log line and audit record must carry.
/// </summary>
public interface IModerationRequestContext
{
    /// <summary>Gets the unique id of this moderation request. Stable across retries.</summary>
    Guid RequestId { get; }

    /// <summary>Gets the correlation id that ties this request across services and log sinks.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the moment the request entered the pipeline (UTC).</summary>
    DateTimeOffset Timestamp { get; }
}
