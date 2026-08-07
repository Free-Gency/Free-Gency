namespace FreeGency.AI.Moderation.Reliability;

/// <summary>
/// Thrown by moderation providers when an AI call fails. <see cref="IsTransient"/>
/// controls whether the retry layer retries; <see cref="Kind"/> controls whether
/// the circuit breaker trips. Client-facing messages must never be built from
/// the exception's <see cref="Exception.Message"/>.
/// </summary>
public sealed class ModerationProviderException : Exception
{
    public ModerationProviderException(ModerationFailureKind kind, string message, bool isTransient, Exception? inner = null)
        : base(message, inner)
    {
        Kind = kind;
        IsTransient = isTransient;
    }

    /// <summary>Gets the failure kind.</summary>
    public ModerationFailureKind Kind { get; }

    /// <summary>Gets a value indicating whether the failure is safe to retry.</summary>
    public bool IsTransient { get; }
}
