namespace FreeGency.AI.Moderation.Reliability;

/// <summary>
/// Classifies a moderation provider failure. <see cref="ModerationProviderException"/>
/// carries a kind so the reliability layer can decide whether a failure is
/// retryable and whether it should trip the circuit breaker.
/// </summary>
public enum ModerationFailureKind
{
    /// <summary>The AI service rejected the request (HTTP 400). Never retried.</summary>
    BadRequest = 0,

    /// <summary>The AI service could not authenticate the request (HTTP 401). Never retried.</summary>
    Unauthorized = 1,

    /// <summary>The AI service denied the request (HTTP 403). Never retried.</summary>
    Forbidden = 2,

    /// <summary>The AI request exceeded the allowed duration. Retryable.</summary>
    Timeout = 3,

    /// <summary>A network-level failure occurred. Retryable.</summary>
    Network = 4,

    /// <summary>The AI response could not be parsed as the expected JSON shape. Retryable.</summary>
    InvalidJson = 5,

    /// <summary>A temporary or unexpected AI failure occurred. Retryable.</summary>
    Internal = 6
}
