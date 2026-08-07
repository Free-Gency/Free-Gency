using System.Diagnostics;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;
using FreeGency.AI.ChatModeration.Interfaces;
using ChatModels = FreeGency.AI.ChatModeration.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Production chat moderation service for the AI Chat Security feature. Reuses the
/// registered <see cref="IChatCompletionService"/> (BedrockGatewayChatService) and the
/// <see cref="IChatSecurityPromptBuilder"/> from Part 3. Never throws; unexpected AI,
/// network, and timeout failures resolve to <see cref="ModerationAction.ManualReview"/>.
/// </summary>
public sealed class ChatModerationService : IChatModerationService
{
    private readonly IChatCompletionService _chat;
    private readonly IChatSecurityPromptBuilder _promptBuilder;
    private readonly MessageNormalizer _normalizer;
    private readonly ContentLanguageDetector _languageDetector;
    private readonly EntityDetector _entityDetector;
    private readonly MessageMasker _masker;
    private readonly ChatSecurityJsonParser _jsonParser;
    private readonly ChatSecurityCache _cache;
    private readonly ChatSecurityMetrics _metrics;
    private readonly ChatModerationOptions _options;
    private readonly ILogger<ChatModerationService> _logger;

    public ChatModerationService(
        IChatCompletionService chat,
        IChatSecurityPromptBuilder promptBuilder,
        MessageNormalizer normalizer,
        ContentLanguageDetector languageDetector,
        EntityDetector entityDetector,
        MessageMasker masker,
        ChatSecurityJsonParser jsonParser,
        ChatSecurityCache cache,
        ChatSecurityMetrics metrics,
        IOptions<ChatModerationOptions> options,
        ILogger<ChatModerationService> logger)
    {
        _chat = chat;
        _promptBuilder = promptBuilder;
        _normalizer = normalizer;
        _languageDetector = languageDetector;
        _entityDetector = entityDetector;
        _masker = masker;
        _jsonParser = jsonParser;
        _cache = cache;
        _metrics = metrics;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatModerationResponse> ModerateAsync(ChatModerationRequest request, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var contentType = ResolveContentType(request);
            var conversationType = ResolveConversationType(request);

            var normalized = _normalizer.Normalize(request.Message);
            if (string.IsNullOrWhiteSpace(normalized))
                return Record(ManualReview("Message is empty.", stopwatch.Elapsed), TimeSpan.Zero);

            var language = _languageDetector.Detect(normalized);

            var cacheKey = _cache.BuildKey(normalized, contentType, conversationType, request);
            if (_options.CacheEnabled && _cache.TryGet(cacheKey, out var cached) && cached is not null)
            {
                stopwatch.Stop();
                _metrics.RecordRequest(cached.Action, usedCache: true, stopwatch.Elapsed, TimeSpan.Zero);
                _logger.LogInformation(
                    "Chat moderation cache hit. Key={CacheKey} ProcessingMs={ProcessingMs}",
                    cacheKey, stopwatch.Elapsed.TotalMilliseconds);
                return cached;
            }

            var context = new ChatModels.ChatModerationContext
            {
                CurrentMessage = normalized,
                PreviousMessages = request.PreviousMessages ?? [],
                ConversationId = request.ConversationId,
                ProjectId = request.ProjectId,
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                SenderRole = request.SenderRole,
                ReceiverRole = request.ReceiverRole
            };

            var prompt = _promptBuilder.Build(context, contentType, conversationType);

            var aiStopwatch = Stopwatch.StartNew();
            var (ai, retryCount, completionLength) = await AskWithRetryAsync(prompt, ct);
            aiStopwatch.Stop();

            if (ai is null)
            {
                _logger.LogWarning(
                    "Moderation failed after {RetryCount} retries. PromptLength={PromptLength} Language={Language} ConversationType={ConversationType}",
                    retryCount, prompt.UserPrompt.Length, language, conversationType);
                return Record(ManualReview("Moderation could not be completed.", stopwatch.Elapsed), aiStopwatch.Elapsed);
            }

            var response = BuildResponse(
                normalized,
                request.Message ?? string.Empty,
                ai,
                language,
                contentType,
                conversationType,
                stopwatch.Elapsed,
                aiStopwatch.Elapsed,
                usedCache: false);

            if (_options.CacheEnabled)
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(_options.CacheExpirationMinutes));

            foreach (var category in response.Categories)
                _metrics.RecordCategory(category.Category.ToString());

            foreach (var keyword in response.MatchedKeywords)
                _metrics.RecordProfanity(keyword);

            _logger.LogInformation(
                "Moderation completed. RiskScore={RiskScore} Confidence={Confidence} Action={Action} IsSafe={IsSafe} " +
                "UsedCache={UsedCache} RetryCount={RetryCount} Language={Language} ConversationType={ConversationType} " +
                "PromptLength={PromptLength} CompletionLength={CompletionLength} ProcessingMs={ProcessingMs} AiLatencyMs={AiLatencyMs}",
                response.RiskScore, response.Confidence, response.Action, response.IsSafe,
                response.UsedCache, retryCount, language, conversationType,
                prompt.UserPrompt.Length, completionLength,
                stopwatch.Elapsed.TotalMilliseconds, aiStopwatch.Elapsed.TotalMilliseconds);

            return Record(response, aiStopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Chat moderation was cancelled.");
            stopwatch.Stop();
            return Record(ManualReview("Moderation was cancelled.", stopwatch.Elapsed), TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure during chat moderation.");
            stopwatch.Stop();
            return Record(ManualReview("Moderation is temporarily unavailable.", stopwatch.Elapsed), TimeSpan.Zero);
        }
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

    private async Task<(ChatSecurityAiResponse? Ai, int RetryCount, int CompletionLength)> AskWithRetryAsync(
        ChatModels.ModerationPrompt prompt, CancellationToken ct)
    {
        var first = await AskModelAsync(prompt, ct);

        if (first.Ai is not null)
            return (first.Ai, 0, first.CompletionLength);

        if (!first.ShouldRetry)
            return (null, 0, first.CompletionLength);

        _logger.LogInformation("Moderation AI returned an unparseable response. Retrying once.");

        var second = await AskModelAsync(prompt, ct);
        return (second.Ai, 1, second.CompletionLength);
    }

    private async Task<(bool ShouldRetry, ChatSecurityAiResponse? Ai, int CompletionLength)> AskModelAsync(
        ChatModels.ModerationPrompt prompt, CancellationToken ct)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(prompt.SystemPrompt);
        history.AddUserMessage(prompt.UserPrompt);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

        try
        {
            var response = await _chat.GetChatMessageContentsAsync(
                history,
                CreateSettings(),
                cancellationToken: timeoutCts.Token);

            var raw = response.FirstOrDefault()?.Content;
            var completionLength = raw?.Length ?? 0;

            if (_jsonParser.TryParse(raw, out var ai) && ai is not null)
                return (false, ai, completionLength);

            return (true, null, completionLength);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Moderation AI call failed.");
            return (false, null, 0);
        }
    }

    private ChatModerationResponse BuildResponse(
        string normalized,
        string original,
        ChatSecurityAiResponse ai,
        ContentLanguage language,
        ContentType contentType,
        ConversationType conversationType,
        TimeSpan processingTime,
        TimeSpan aiLatency,
        bool usedCache)
    {
        var entities = _entityDetector.Detect(original);
        var profanity = _masker.FindProfanity(normalized);

        var riskScore = Math.Round(
            ai.RiskScore is double risk ? Math.Clamp(risk, 0, 100) : DeriveRiskScore(ai, profanity.Count > 0));
        var confidence = ai.Confidence is double conf ? Math.Clamp(conf, 0, 1) : 0.5;
        var riskLevel = MapRiskLevel(riskScore);
        var action = MapAction(riskScore);

        var categories = BuildCategories(ai, entities, profanity);

        var keywords = new List<string>();
        foreach (var keyword in ai.MatchedKeywords)
        {
            if (!keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
                keywords.Add(keyword);
        }
        foreach (var keyword in profanity)
        {
            if (!keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
                keywords.Add(keyword);
        }

        var localMasked = _masker.Mask(original, entities, profanity);
        var hasMasking = !string.Equals(localMasked, original, StringComparison.Ordinal);
        var maskedMessage = hasMasking
            ? localMasked
            : string.IsNullOrWhiteSpace(ai.MaskedMessage) ? null : ai.MaskedMessage;

        return new ChatModerationResponse
        {
            IsSafe = riskScore <= 20,
            RiskScore = riskScore,
            Confidence = confidence,
            RiskLevel = riskLevel,
            Action = action,
            Reason = string.IsNullOrWhiteSpace(ai.Reason) ? BuildDefaultReason(action, riskScore) : ai.Reason,
            Summary = null,
            Categories = categories,
            DetectedEntities = entities,
            DetectedLanguages = MapLanguages(language),
            MatchedKeywords = keywords,
            MaskedMessage = maskedMessage,
            ProcessingTimeMs = (long)processingTime.TotalMilliseconds,
            ModelName = _options.ModelId,
            UsedCache = usedCache,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private IReadOnlyList<CategoryScoreDto> BuildCategories(
        ChatSecurityAiResponse ai,
        IReadOnlyList<DetectedEntityDto> entities,
        IReadOnlyList<string> profanity)
    {
        var categories = new List<CategoryScoreDto>();

        foreach (var category in ai.Categories)
        {
            if (string.IsNullOrWhiteSpace(category.Category))
                continue;

            if (Enum.TryParse<ModerationCategory>(category.Category, ignoreCase: true, out var parsed))
                categories.Add(new CategoryScoreDto(parsed, Math.Clamp(category.Score ?? 0.5, 0, 1)));
        }

        if (profanity.Count > 0 && !categories.Any(c => c.Category == ModerationCategory.Profanity))
            categories.Add(new CategoryScoreDto(ModerationCategory.Profanity, 0.8));

        var hasSensitive = entities.Any(e => e.Type is DetectedEntityType.Phone or DetectedEntityType.Email
            or DetectedEntityType.CreditCard or DetectedEntityType.IBAN or DetectedEntityType.Passport
            or DetectedEntityType.NationalId or DetectedEntityType.Password or DetectedEntityType.CryptoWallet);

        if (hasSensitive && !categories.Any(c => c.Category == ModerationCategory.SensitiveInformation))
            categories.Add(new CategoryScoreDto(ModerationCategory.SensitiveInformation, 0.9));

        var hasExternal = entities.Any(e => e.Type is DetectedEntityType.Telegram or DetectedEntityType.WhatsApp
            or DetectedEntityType.Discord or DetectedEntityType.Facebook or DetectedEntityType.Instagram
            or DetectedEntityType.LinkedIn or DetectedEntityType.Twitter or DetectedEntityType.GitHub
            or DetectedEntityType.URL);

        if (hasExternal && !categories.Any(c => c.Category == ModerationCategory.ExternalContact))
            categories.Add(new CategoryScoreDto(ModerationCategory.ExternalContact, 0.7));

        return categories;
    }

    private static double DeriveRiskScore(ChatSecurityAiResponse ai, bool hasProfanity)
    {
        if (Enum.TryParse<RiskLevel>(ai.RiskLevel, ignoreCase: true, out var level))
        {
            return level switch
            {
                RiskLevel.Safe => 10,
                RiskLevel.Low => 30,
                RiskLevel.Medium => 50,
                RiskLevel.High => 70,
                _ => 90
            };
        }

        if (ai.Categories.Count > 0)
        {
            var maxScore = ai.Categories.Max(c => Math.Clamp(c.Score ?? 0, 0, 1));
            if (maxScore > 0)
                return maxScore * 100;
        }

        return hasProfanity ? 60 : 10;
    }

    private static RiskLevel MapRiskLevel(double score)
    {
        return score switch
        {
            <= 20 => RiskLevel.Safe,
            <= 40 => RiskLevel.Low,
            <= 60 => RiskLevel.Medium,
            <= 80 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }

    private static ModerationAction MapAction(double score)
    {
        return score switch
        {
            > 90 => ModerationAction.Reject,
            > 75 => ModerationAction.Mask,
            > 50 => ModerationAction.Warn,
            _ => ModerationAction.Allow
        };
    }

    private static IReadOnlyList<string> MapLanguages(ContentLanguage language)
    {
        return language switch
        {
            ContentLanguage.Arabic => ["ar"],
            ContentLanguage.English => ["en"],
            ContentLanguage.Franco => ["ar-latn"],
            ContentLanguage.Mixed => ["ar", "en"],
            ContentLanguage.Emoji => ["emoji"],
            ContentLanguage.Programming => ["code"],
            _ => []
        };
    }

    private static string BuildDefaultReason(ModerationAction action, double riskScore)
    {
        return action switch
        {
            ModerationAction.Reject => $"Content was rejected due to high risk (risk score {riskScore:0}).",
            ModerationAction.Mask => $"Content requires masking of sensitive data (risk score {riskScore:0}).",
            ModerationAction.Warn => $"Content contains potentially inappropriate material (risk score {riskScore:0}).",
            ModerationAction.ManualReview => "Content requires manual review.",
            _ => "Content appears safe."
        };
    }

    private static ContentType ResolveContentType(ChatModerationRequest request)
    {
        if (request.Metadata is not null &&
            request.Metadata.TryGetValue("ContentType", out var value) &&
            Enum.TryParse<ContentType>(value, ignoreCase: true, out var contentType))
        {
            return contentType;
        }

        return ContentType.ChatMessage;
    }

    private static ConversationType ResolveConversationType(ChatModerationRequest request)
    {
        if (request.Metadata is not null &&
            request.Metadata.TryGetValue("ConversationType", out var value) &&
            Enum.TryParse<ConversationType>(value, ignoreCase: true, out var conversationType))
        {
            return conversationType;
        }

        return ConversationType.PrivateChat;
    }

    private ChatModerationResponse ManualReview(string reason, TimeSpan elapsed)
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
            ModelName = _options.ModelId,
            UsedCache = false,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private ChatModerationResponse Record(ChatModerationResponse response, TimeSpan aiLatency)
    {
        _metrics.RecordRequest(response.Action, response.UsedCache, TimeSpan.FromMilliseconds(response.ProcessingTimeMs), aiLatency);
        return response;
    }

    private PromptExecutionSettings CreateSettings()
    {
        return new PromptExecutionSettings
        {
            ModelId = _options.ModelId,
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = _options.Temperature,
                ["max_tokens"] = _options.MaxTokens
            }
        };
    }
}
