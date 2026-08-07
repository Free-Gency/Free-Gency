using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FreeGency.AI.Moderation.Enums;
using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Providers;
using FreeGency.AI.Moderation.Reliability;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Moderation.Observability;

/// <summary>
/// Observability decorator over <see cref="IModerationProvider"/>. Logs every
/// request using only a SHA-256 content hash and lengths (never raw content),
/// records metrics, and redacts any sensitive values that might reach log output
/// via <see cref="ModerationLogSanitizer"/>.
/// </summary>
public sealed class LoggingModerationProvider : IModerationProvider
{
    private readonly IModerationProvider _inner;
    private readonly ILogger<LoggingModerationProvider> _logger;
    private readonly ModerationMetrics _metrics;

    public LoggingModerationProvider(
        IModerationProvider inner,
        ILogger<LoggingModerationProvider> logger,
        ModerationMetrics metrics)
    {
        _inner = inner;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();
        _metrics.RecordRequestStarted();
        var contentHash = ComputeHash(request.Content);

        try
        {
            _logger.LogInformation(
                "Moderation started. ContentHash={ContentHash} ContentLength={ContentLength} ContentKind={ContentKind}",
                contentHash, request.Content.Length, request.ContentKind);

            var result = await _inner.AnalyzeAsync(request, ct);

            stopwatch.Stop();
            LogResult(result, stopwatch.Elapsed, contentHash);
            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _metrics.RecordFailure();
            throw;
        }
        catch (ModerationProviderException ex)
        {
            stopwatch.Stop();
            _metrics.RecordFailure();
            _logger.LogWarning(
                "Moderation failed. Kind={Kind} ElapsedMs={ElapsedMs} ContentHash={ContentHash}",
                ex.Kind, stopwatch.Elapsed.TotalMilliseconds, contentHash);
            throw;
        }
    }

    private void LogResult(ModerationResult result, TimeSpan elapsed, string contentHash)
    {
        var analysis = result.Analysis;

        if (analysis is not null)
        {
            _metrics.RecordAiCall(elapsed);
            _metrics.RecordAction(analysis.Action);
            foreach (var category in analysis.Categories)
                _metrics.RecordCategory(category.Category);

            _logger.LogInformation(
                "Moderation completed. RiskScore={RiskScore} Confidence={Confidence} Action={Action} " +
                "RiskLevel={RiskLevel} FromCache={FromCache} RetryCount={RetryCount} CircuitBroken={CircuitBroken} " +
                "ElapsedMs={ElapsedMs} ContentHash={ContentHash}",
                analysis.RiskScore, analysis.Confidence, analysis.Action, analysis.RiskLevel,
                result.FromCache, result.RetryCount, result.CircuitBroken,
                elapsed.TotalMilliseconds, contentHash);
        }
        else
        {
            _metrics.RecordAction(ModerationAction.ManualReview);
            _logger.LogWarning(
                "Moderation result requires manual review. Error={Error} CircuitBroken={CircuitBroken} " +
                "ElapsedMs={ElapsedMs} ContentHash={ContentHash}",
                result.Error is null ? null : ModerationLogSanitizer.Sanitize(result.Error),
                result.CircuitBroken, elapsed.TotalMilliseconds, contentHash);
        }
    }

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
