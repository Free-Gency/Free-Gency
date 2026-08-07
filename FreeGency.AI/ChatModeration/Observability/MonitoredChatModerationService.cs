using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FreeGency.AI.ChatModeration.Constants;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;
using FreeGency.AI.ChatModeration.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Observability decorator for the moderation pipeline. Wraps an inner
/// <see cref="IChatModerationService"/> and, for every request, emits audit
/// records, structured event logs, and metrics, and evaluates performance
/// alerts. It never modifies the moderation outcome. Never throws; failures are
/// handled by the inner service.
/// </summary>
public sealed class MonitoredChatModerationService : IChatModerationService
{
    private static readonly HashSet<ModerationCategory> ScamCategories =
    [
        ModerationCategory.Scam,
        ModerationCategory.CryptoScam,
        ModerationCategory.FakeSupport,
        ModerationCategory.Fraud,
        ModerationCategory.Phishing,
        ModerationCategory.SocialEngineering
    ];

    private static readonly HashSet<ModerationCategory> PromptInjectionCategories =
    [
        ModerationCategory.PromptInjection,
        ModerationCategory.Jailbreak
    ];

    private readonly IChatModerationService _inner;
    private readonly IChatModerationLogger _logger;
    private readonly ChatModerationMetrics _metrics;
    private readonly ChatModerationMonitoringOptions _options;
    private readonly MessageNormalizer _normalizer;
    private readonly ContentLanguageDetector _languageDetector;
    private readonly IOptions<ChatModerationCacheOptions> _cacheOptions;

    public MonitoredChatModerationService(
        IChatModerationService inner,
        IChatModerationLogger logger,
        ChatModerationMetrics metrics,
        IOptions<ChatModerationMonitoringOptions> options,
        MessageNormalizer normalizer,
        ContentLanguageDetector languageDetector,
        IOptions<ChatModerationCacheOptions> cacheOptions)
    {
        _inner = inner;
        _logger = logger;
        _metrics = metrics;
        _options = options.Value;
        _normalizer = normalizer;
        _languageDetector = languageDetector;
        _cacheOptions = cacheOptions;
    }

    public async Task<ChatModerationResponse> ModerateAsync(ChatModerationRequest request, CancellationToken ct = default)
    {
        var totalStopwatch = Stopwatch.StartNew();

        var normalized = _normalizer.Normalize(request.Message);
        var language = _languageDetector.Detect(normalized);
        var messageHash = ComputeHash(request.Message);
        var context = _logger.BeginRequest(request, messageHash, language, ChatSecurityCacheConstants.PromptVersion);

        _metrics.RecordRequestStart();
        _metrics.MarkCacheAvailable(_cacheOptions.Value.CacheEnabled);
        _logger.LogRequestStarted(context);

        var aiStopwatch = Stopwatch.StartNew();
        ChatModerationResponse response;

        try
        {
            _logger.LogAiStarted(context);
            response = await _inner.ModerateAsync(request, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            aiStopwatch.Stop();
            totalStopwatch.Stop();
            _logger.LogCancelled(context);
            _metrics.RecordCompleted(usedCache: false, totalStopwatch.Elapsed);
            _metrics.RecordAction(ModerationAction.ManualReview);
            return BuildFallback("Moderation was cancelled.", totalStopwatch.Elapsed);
        }
        catch (Exception exception)
        {
            aiStopwatch.Stop();
            totalStopwatch.Stop();
            _logger.LogUnexpectedException(context, exception);
            _metrics.RecordCompleted(usedCache: false, totalStopwatch.Elapsed);
            _metrics.RecordFailure();
            _metrics.RecordAction(ModerationAction.ManualReview);
            _metrics.MarkAiAvailable(false);
            return BuildFallback("Moderation is temporarily unavailable.", totalStopwatch.Elapsed);
        }
        finally
        {
            aiStopwatch.Stop();
        }

        aiStopwatch.Stop();
        totalStopwatch.Stop();

        var totalTime = totalStopwatch.Elapsed;
        var aiTime = response.UsedCache ? TimeSpan.Zero : aiStopwatch.Elapsed;

        _logger.LogAiFinished(context, aiTime, response.UsedCache);

        RecordMetrics(response, language, aiTime, totalTime);
        LogOutcome(context, response, aiTime);
        LogCache(context, response, aiStopwatch.Elapsed);

        _logger.LogAudit(context, response, totalTime, retryCount: 0);

        if (_options.EnablePerformanceLogs)
            _logger.LogPerformanceAlerts(_metrics, aiTime);

        return response;
    }

    public async Task<bool> IsSafeAsync(string message, CancellationToken ct = default)
    {
        var response = await ModerateAsync(new ChatModerationRequest { Message = message }, ct);
        return response.IsSafe;
    }

    public async Task<double> CalculateRiskScoreAsync(ChatModerationRequest request, CancellationToken ct = default)
    {
        var response = await ModerateAsync(request, ct);
        return response.RiskScore;
    }

    private void RecordMetrics(ChatModerationResponse response, ContentLanguage language, TimeSpan aiTime, TimeSpan totalTime)
    {
        if (!_options.EnableMetrics)
            return;

        _metrics.RecordCompleted(response.UsedCache, totalTime);
        _metrics.RecordAiCall(aiTime);
        _metrics.RecordAction(response.Action);
        _metrics.RecordLanguage(language.ToString());

        _metrics.MarkAiAvailable(response.Action != ModerationAction.ManualReview);

        foreach (var category in response.Categories)
        {
            var name = category.Category.ToString();
            _metrics.RecordCategory(name);

            if (ScamCategories.Contains(category.Category))
                _metrics.RecordScamType(name);

            if (PromptInjectionCategories.Contains(category.Category))
                _metrics.RecordPromptInjection(name);
        }

        foreach (var keyword in response.MatchedKeywords)
            _metrics.RecordProfanity(keyword);

        foreach (var entity in response.DetectedEntities)
            _metrics.RecordSensitiveEntity(entity.Type.ToString());
    }

    private void LogOutcome(ChatModerationRequestContext context, ChatModerationResponse response, TimeSpan aiTime)
    {
        if (response.Action == ModerationAction.ManualReview)
        {
            _logger.LogManualReview(context, response);

            var reason = response.Reason ?? string.Empty;
            if (reason.Contains("could not be completed", StringComparison.OrdinalIgnoreCase) ||
                reason.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogAiFailure(context, reason, aiTime);
                _metrics.RecordFailure();
                _metrics.MarkAiAvailable(false);
            }

            return;
        }

        switch (response.Action)
        {
            case ModerationAction.Allow:
                _logger.LogAllowed(context, response);
                break;
            case ModerationAction.Warn:
                _logger.LogWarned(context, response);
                break;
            case ModerationAction.Mask:
                _logger.LogMasked(context, response);
                break;
            case ModerationAction.Reject:
                _logger.LogRejected(context, response);
                break;
        }

        if (response.RiskScore >= _options.HighRiskThreshold)
            _logger.LogHighRisk(context, response);
    }

    private void LogCache(ChatModerationRequestContext context, ChatModerationResponse response, TimeSpan lookupTime)
    {
        if (response.UsedCache)
            _logger.LogCacheHit(context, lookupTime);
        else
            _logger.LogCacheMiss(context);
    }

    private ChatModerationResponse BuildFallback(string reason, TimeSpan elapsed)
    {
        return new ChatModerationResponse
        {
            IsSafe = false,
            RiskScore = 50,
            Confidence = 0,
            RiskLevel = RiskLevel.Medium,
            Action = ModerationAction.ManualReview,
            Reason = reason,
            Summary = null,
            Categories = [],
            DetectedEntities = [],
            DetectedLanguages = [],
            MatchedKeywords = [],
            MaskedMessage = null,
            ProcessingTimeMs = (long)elapsed.TotalMilliseconds,
            ModelName = null,
            UsedCache = false,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private static string ComputeHash(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(message))).ToLowerInvariant();
    }
}
