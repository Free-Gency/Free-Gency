namespace FreeGency.AI.Monitoring;

/// <summary>
/// Default <see cref="ICorrelationContextAccessor"/> backed by an
/// <see cref="AsyncLocal{T}"/>. The value flows with the async execution context:
/// a request sets it on entry and clears it when it finishes, so concurrent
/// requests each observe their own correlation ids while the same singleton
/// instance is shared across the process.
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    /// <inheritdoc />
    public CorrelationContext? Current => CurrentContext.Value;

    /// <inheritdoc />
    public void Set(CorrelationContext context) => CurrentContext.Value = context;

    /// <inheritdoc />
    public void Clear() => CurrentContext.Value = null;
}
