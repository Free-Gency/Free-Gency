using System.Diagnostics;
using FreeGency.AI.Moderation.Enums;
using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Observability;
using FreeGency.AI.Moderation.Providers;
using FreeGency.AI.Moderation.Reliability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Moderation;

/// <summary>
/// The default <see cref="IModerationService"/> implementation. Delegates to
/// the decorated <see cref="IModerationProvider"/> chain and acts as the final
/// safety net: empty or oversized content and any remaining provider failure
/// resolve to a manual-review result. Never throws to callers.
/// </summary>
public sealed class ModerationService : IModerationService
{
    private readonly IModerationProvider _provider;
    private readonly ModerationOptions _options;
    private readonly ILogger<ModerationService> _logger;

    public ModerationService(
        IModerationProvider provider,
        IOptions<ModerationOptions> options,
        ILogger<ModerationService> logger)
    {
        _provider = provider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ModerationResult> ModerateAsync(ModerationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var content = request.Content.Trim();

            if (content.Length == 0)
                return Finish(ModerationResult.FromAnalysis(SafeAnalysis("Content is empty.")), stopwatch);

            if (content.Length > _options.MaxContentLength)
            {
                _logger.LogWarning(
                    "Moderation content is too long. ContentLength={ContentLength} MaxContentLength={MaxContentLength}",
                    content.Length, _options.MaxContentLength);
                return Finish(ModerationResult.ManualReview("Content exceeds the maximum allowed length."), stopwatch);
            }

            var result = await _provider.AnalyzeAsync(request, ct);
            return Finish(result, stopwatch);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Moderation was cancelled.");
            return Finish(ModerationResult.ManualReview("Moderation was cancelled."), stopwatch);
        }
        catch (ModerationProviderException ex)
        {
            _logger.LogWarning(
                "Moderation provider failed. Kind={Kind} Message={Message}",
                ex.Kind, ModerationLogSanitizer.Sanitize(ex.Message));
            return Finish(ModerationResult.ManualReview("Moderation is temporarily unavailable."), stopwatch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure during moderation.");
            return Finish(ModerationResult.ManualReview("Moderation is temporarily unavailable."), stopwatch);
        }
    }

    public async Task<bool> IsSafeAsync(string content, CancellationToken ct = default)
    {
        var result = await ModerateAsync(new ModerationRequest { Content = content }, ct);
        return result.Analysis?.IsSafe ?? false;
    }

    public async Task<double> CalculateRiskScoreAsync(ModerationRequest request, CancellationToken ct = default)
    {
        var result = await ModerateAsync(request, ct);
        return result.Analysis?.RiskScore ?? 50;
    }

    private ModerationAnalysis SafeAnalysis(string reason)
    {
        return new ModerationAnalysis(
            IsSafe: true,
            RiskScore: 0,
            Confidence: 1,
            RiskLevel: RiskLevel.Safe,
            Action: ModerationAction.Allow,
            Reason: reason,
            Categories: [],
            PromptVersion: _options.PromptVersion,
            ModelName: _options.ModelId);
    }

    private static ModerationResult Finish(ModerationResult result, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return result with
        {
            ElapsedMs = stopwatch.ElapsedMilliseconds,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
