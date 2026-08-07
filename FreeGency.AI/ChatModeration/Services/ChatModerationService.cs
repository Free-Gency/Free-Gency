using System.Diagnostics;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Models;
using FreeGency.AI.ChatModeration.Validators;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.ChatModeration.Services;

public sealed class ChatModerationService : IChatModerationService
{
    private readonly IChatCompletionService _chat;
    private readonly IChatModerationPromptBuilder _promptBuilder;
    private readonly IChatModerationJsonParser _jsonParser;
    private readonly IChatModerationCache _cache;
    private readonly IChatModerationLogger _logger;
    private readonly IChatModerationMetrics _metrics;
    private readonly ChatModerationOptions _options;
    private readonly ModerationRequestValidator _validator;

    public ChatModerationService(
        IChatCompletionService chat,
        IChatModerationPromptBuilder promptBuilder,
        IChatModerationJsonParser jsonParser,
        IChatModerationCache cache,
        IChatModerationLogger logger,
        IChatModerationMetrics metrics,
        IOptions<ChatModerationOptions> options)
    {
        _chat = chat;
        _promptBuilder = promptBuilder;
        _jsonParser = jsonParser;
        _cache = cache;
        _logger = logger;
        _metrics = metrics;
        _options = options.Value;
        _validator = new ModerationRequestValidator(_options);
    }

    public async Task<ModerationResult> ModerateAsync(ModerationRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var key = _cache.BuildKey(request);

        if (!_validator.Validate(request, out var validationError))
        {
            return await FailAsync(key, sw, ModerationDecision.Block,
                "Invalid moderation request.", validationError, ct);
        }

        if (_options.CacheEnabled && !request.BypassCache)
        {
            var cached = await _cache.TryGetAsync(key, ct);
            if (cached is not null)
            {
                sw.Stop();
                _logger.LogCacheHit(key);
                _metrics.RecordRequest(cached.Decision.ToString(), cached.Confidence, cached.Elapsed, fromCache: true);
                return cached;
            }

            _logger.LogCacheMiss(key);
        }

        _logger.LogRequestStarted(key);

        try
        {
            var prompt = _promptBuilder.Build(request);
            var aiResult = await AskModelAsync(prompt, ct);

            var result = MapToResult(request, aiResult, sw.Elapsed);

            if (_options.CacheEnabled)
                await _cache.SetAsync(key, result, TimeSpan.FromMinutes(_options.CacheExpirationMinutes), ct);

            _metrics.RecordRequest(result.Decision.ToString(), result.Confidence, result.Elapsed, fromCache: false);
            _logger.LogRequestCompleted(key, result.Decision.ToString(), result.Elapsed);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(key, ex);
            return await FailAsync(key, sw, ModerationDecision.Block,
                "Moderation service is temporarily unavailable.", ex.Message, ct);
        }
    }

    public async Task<ModerationReport> ModerateBatchAsync(IEnumerable<ModerationRequest> requests, CancellationToken ct = default)
    {
        var requestsList = requests.ToList();
        var results = new List<ModerationResult>(requestsList.Count);

        using var semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);

        var tasks = requestsList.Select(async request =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await ModerateAsync(request, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        results.AddRange(await Task.WhenAll(tasks));

        return new ModerationReport
        {
            TotalCount = results.Count,
            AllowedCount = results.Count(r => r.Decision == ModerationDecision.Allow),
            FlaggedCount = results.Count(r => r.Decision == ModerationDecision.Flag),
            RequireReviewCount = results.Count(r => r.Decision == ModerationDecision.RequireReview),
            BlockedCount = results.Count(r => r.Decision == ModerationDecision.Block),
            Results = results
        };
    }

    private async Task<ModerationAiResult> AskModelAsync(ModerationPrompt prompt, CancellationToken ct)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(prompt.SystemPrompt);
        history.AddUserMessage(prompt.UserPrompt);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

        var settings = new PromptExecutionSettings
        {
            ModelId = _options.ModelId,
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = _options.Temperature,
                ["max_tokens"] = _options.MaxTokens
            }
        };

        var response = await _chat.GetChatMessageContentsAsync(
            history,
            settings,
            cancellationToken: timeoutCts.Token);

        var raw = response.FirstOrDefault()?.Content;

        if (_jsonParser.TryParse(raw, out var result) && result is not null)
            return result;

        _logger.LogParseFailure(BuildKeyForPrompt(prompt));

        return new ModerationAiResult
        {
            Decision = "block",
            Severity = "high",
            Confidence = 0,
            Summary = "Moderation service returned an unparseable response. Blocking as a safety precaution.",
            Categories = []
        };
    }

    private string BuildKeyForPrompt(ModerationPrompt prompt)
    {
        var request = new ModerationRequest
        {
            Content = prompt.UserPrompt,
            ContentType = ContentType.ChatMessage
        };

        return _cache.BuildKey(request);
    }

    private ModerationResult MapToResult(ModerationRequest request, ModerationAiResult aiResult, TimeSpan elapsed)
    {
        var decision = ParseDecision(aiResult.Decision);
        var severity = ParseSeverity(aiResult.Severity);
        var categories = (aiResult.Categories ?? [])
            .Select(c => new ModerationCategoryResult
            {
                Name = string.IsNullOrWhiteSpace(c.Name) ? "other" : c.Name,
                Severity = ParseSeverity(c.Severity),
                Score = Math.Clamp(c.Score, 0, 1)
            })
            .ToList();

        if (categories.Count > 0)
        {
            var maxSeverity = categories.Max(c => c.Severity);
            if (maxSeverity > severity)
                severity = maxSeverity;
        }

        return new ModerationResult
        {
            ContentHash = request.Content,
            Decision = decision,
            Severity = severity,
            Confidence = Math.Clamp(aiResult.Confidence, 0, 1),
            IsApproved = decision == ModerationDecision.Allow,
            FromCache = false,
            Categories = categories,
            Summary = aiResult.Summary,
            SanitizedContent = aiResult.SanitizedContent,
            ModelUsed = _options.ModelId,
            Elapsed = elapsed
        };
    }

    private async Task<ModerationResult> FailAsync(
        string key,
        Stopwatch sw,
        ModerationDecision decision,
        string summary,
        string detail,
        CancellationToken ct)
    {
        sw.Stop();
        _logger.LogRequestCompleted(key, decision.ToString(), sw.Elapsed);

        var result = new ModerationResult
        {
            ContentHash = key,
            Decision = decision,
            Severity = ModerationSeverity.Critical,
            Confidence = 0,
            IsApproved = false,
            FromCache = false,
            Categories = [],
            Summary = summary,
            ModelUsed = _options.ModelId,
            Elapsed = sw.Elapsed
        };

        _metrics.RecordRequest(decision.ToString(), 0, sw.Elapsed, fromCache: false);
        return result;
    }

    private static ModerationDecision ParseDecision(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "allow" => ModerationDecision.Allow,
            "flag" => ModerationDecision.Flag,
            "review" => ModerationDecision.RequireReview,
            "block" => ModerationDecision.Block,
            _ => ModerationDecision.Block
        };
    }

    private static ModerationSeverity ParseSeverity(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "safe" => ModerationSeverity.Safe,
            "low" => ModerationSeverity.Low,
            "medium" => ModerationSeverity.Medium,
            "high" => ModerationSeverity.High,
            "critical" => ModerationSeverity.Critical,
            _ => ModerationSeverity.Low
        };
    }
}
