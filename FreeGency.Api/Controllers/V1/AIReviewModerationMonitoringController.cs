using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.Monitoring;
using FreeGency.AI.ReviewModeration.Observability;
using FreeGency.Application.Common.Results;
using Microsoft.Extensions.Options;

namespace FreeGency.Api.Controllers.V1;

/// <summary>
/// AI Review Moderation monitoring API.
///
/// Exposes the read-only <c>GET /api/v1/ai/reviews/health</c> and
/// <c>GET /api/v1/ai/reviews/metrics</c> endpoints used by operators and load
/// balancers to observe the review moderation pipeline without touching the
/// moderation flow itself.
/// </summary>
[Route("api/v1/ai/reviews")]
[Authorize]
public class AIReviewModerationMonitoringController(
    IReviewModerationHealthCheck healthCheck,
    IReviewModerationMetrics metrics,
    IOptions<ReviewModerationCacheOptions> options) : BaseApiController
{
    /// <summary>
    /// Reports the current health of the review moderation pipeline.
    ///
    /// Combines the observed AI availability and failure rate with the live
    /// circuit breaker state, the cache configuration, the prompt loader, and
    /// the bound options into a single verdict. Returns "Healthy", "Degraded",
    /// or "Unhealthy" together with the underlying signals. This endpoint never
    /// calls the AI service; it only reads in-process telemetry, so it is safe
    /// to probe frequently.
    /// </summary>
    /// <returns>200 with the health report; 401/403 when the caller is not
    /// authenticated or authorized.</returns>
    [HttpGet("health")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ReviewModerationHealth>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public IActionResult GetHealth()
        => HandleResult(ApiResponse.Success(healthCheck.Evaluate()));

    /// <summary>
    /// Reports the current telemetry snapshot for the review moderation pipeline.
    ///
    /// Includes request totals, cache hit rate, AI latency percentiles, outcome
    /// action counters, success/failure and retry rates, security category hits,
    /// and the estimated cost saved by cache hits. The snapshot is immutable and
    /// contains only aggregates; no review content is exposed.
    /// </summary>
    /// <returns>200 with the metrics snapshot; 401/403 when the caller is not
    /// authenticated or authorized.</returns>
    [HttpGet("metrics")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ReviewModerationMetricsSnapshot>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public IActionResult GetMetrics()
        => HandleResult(ApiResponse.Success(metrics.GetSnapshot(options.Value.EstimatedCostPerAiCallUsd)));
}
