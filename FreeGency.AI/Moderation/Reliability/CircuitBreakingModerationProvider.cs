using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Providers;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Moderation.Reliability;

/// <summary>
/// Protects the moderation pipeline from repeated AI failures. When the
/// <see cref="ModerationCircuitBreaker"/> is open, calls short-circuit to a
/// manual-review result without touching the AI service. Each failure after the
/// retry layer has been exhausted records a failure on the breaker; successes
/// close the circuit.
/// </summary>
public sealed class CircuitBreakingModerationProvider : IModerationProvider
{
    private readonly IModerationProvider _inner;
    private readonly ModerationCircuitBreaker _circuitBreaker;
    private readonly ILogger<CircuitBreakingModerationProvider> _logger;
    private readonly ModerationMetrics _metrics;

    public CircuitBreakingModerationProvider(
        IModerationProvider inner,
        ModerationCircuitBreaker circuitBreaker,
        ILogger<CircuitBreakingModerationProvider> logger,
        ModerationMetrics metrics)
    {
        _inner = inner;
        _circuitBreaker = circuitBreaker;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default)
    {
        if (_circuitBreaker.IsOpen)
        {
            _metrics.RecordCircuitTrip();
            _logger.LogWarning(
                "Moderation circuit breaker is open (state={State}). Returning manual review without calling the AI service.",
                _circuitBreaker.State);
            return ModerationResult.ManualReview(
                "Moderation is temporarily unavailable.",
                circuitBroken: true);
        }

        try
        {
            var result = await _inner.AnalyzeAsync(request, ct);
            _circuitBreaker.RecordSuccess();
            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (ModerationProviderException ex)
        {
            _circuitBreaker.RecordFailure();
            _metrics.RecordFailure();
            _logger.LogWarning(
                "Moderation provider failed. Kind={Kind} CircuitState={CircuitState}",
                ex.Kind, _circuitBreaker.State);
            throw;
        }
        catch (Exception ex)
        {
            _circuitBreaker.RecordFailure();
            _metrics.RecordFailure();
            _logger.LogError(ex, "Unexpected failure from the inner moderation provider.");
            throw new ModerationProviderException(
                ModerationFailureKind.Internal,
                "The moderation provider failed unexpectedly.",
                isTransient: true,
                ex);
        }
    }
}
