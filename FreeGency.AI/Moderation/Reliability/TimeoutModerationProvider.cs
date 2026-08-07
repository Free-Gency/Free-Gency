using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Providers;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Moderation.Reliability;

/// <summary>
/// Enforces a per-request timeout on the wrapped provider. When the AI call
/// exceeds the configured duration, the call is cancelled and converted to a
/// transient <see cref="ModerationProviderException"/> with
/// <see cref="ModerationFailureKind.Timeout"/>. Caller-initiated cancellations
/// propagate untouched.
/// </summary>
public sealed class TimeoutModerationProvider : IModerationProvider
{
    private readonly IModerationProvider _inner;
    private readonly TimeSpan _timeout;
    private readonly ILogger<TimeoutModerationProvider> _logger;
    private readonly ModerationMetrics _metrics;

    public TimeoutModerationProvider(
        IModerationProvider inner,
        TimeSpan timeout,
        ILogger<TimeoutModerationProvider> logger,
        ModerationMetrics metrics)
    {
        _inner = inner;
        _timeout = timeout;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        try
        {
            return await _inner.AnalyzeAsync(request, cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Moderation AI call timed out after {TimeoutSeconds} seconds.", _timeout.TotalSeconds);
            _metrics.RecordTimeout();
            throw new ModerationProviderException(
                ModerationFailureKind.Timeout,
                "The moderation AI request timed out.",
                isTransient: true);
        }
    }
}
