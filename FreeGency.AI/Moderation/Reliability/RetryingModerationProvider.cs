using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Providers;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Moderation.Reliability;

/// <summary>
/// Retries the wrapped provider when it fails transiently (timeout, network,
/// invalid JSON, or a temporary AI failure). Permanent failures
/// (<see cref="ModerationFailureKind.BadRequest"/>, <see cref="ModerationFailureKind.Unauthorized"/>,
/// <see cref="ModerationFailureKind.Forbidden"/>) are never retried. The number of
/// retries is configured via <c>ModerationOptions.RetryCount</c> (default 1).
/// </summary>
public sealed class RetryingModerationProvider : IModerationProvider
{
    private readonly IModerationProvider _inner;
    private readonly int _retryCount;
    private readonly ILogger<RetryingModerationProvider> _logger;
    private readonly ModerationMetrics _metrics;

    public RetryingModerationProvider(
        IModerationProvider inner,
        int retryCount,
        ILogger<RetryingModerationProvider> logger,
        ModerationMetrics metrics)
    {
        _inner = inner;
        _retryCount = Math.Max(0, retryCount);
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                return await _inner.AnalyzeAsync(request, ct);
            }
            catch (ModerationProviderException ex) when (ex.IsTransient && attempt < _retryCount && !ct.IsCancellationRequested)
            {
                attempt++;
                _metrics.RecordRetry();
                _logger.LogWarning(
                    "Moderation provider failed transiently. Attempt={Attempt} MaxRetries={MaxRetries} Kind={Kind}",
                    attempt, _retryCount, ex.Kind);
            }
        }
    }
}
