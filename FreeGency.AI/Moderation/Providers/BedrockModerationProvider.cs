using System.Net;
using FreeGency.AI.Moderation.Enums;
using FreeGency.AI.Moderation.Models;
using FreeGency.AI.Moderation.Prompts;
using FreeGency.AI.Moderation.Reliability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.Moderation.Providers;

/// <summary>
/// The real AI moderation provider. Calls the registered
/// <see cref="IChatCompletionService"/> (the Bedrock gateway) with the prompt
/// built by <see cref="IModerationPromptBuilder"/> and maps the model's JSON
/// reply into a <see cref="ModerationAnalysis"/>. It never depends on
/// <c>BedrockGatewayChatService</c> directly, only on the SK abstraction.
/// </summary>
public sealed class BedrockModerationProvider : IModerationProvider
{
    private readonly IChatCompletionService _chat;
    private readonly IModerationPromptBuilder _promptBuilder;
    private readonly ModerationJsonParser _jsonParser;
    private readonly ModerationOptions _options;
    private readonly ILogger<BedrockModerationProvider> _logger;

    public BedrockModerationProvider(
        IChatCompletionService chat,
        IModerationPromptBuilder promptBuilder,
        ModerationJsonParser jsonParser,
        IOptions<ModerationOptions> options,
        ILogger<BedrockModerationProvider> logger)
    {
        _chat = chat;
        _promptBuilder = promptBuilder;
        _jsonParser = jsonParser;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ModerationResult> AnalyzeAsync(ModerationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prompt = _promptBuilder.Build(request);

        var history = new ChatHistory();
        history.AddSystemMessage(prompt.SystemPrompt);
        history.AddUserMessage(prompt.UserPrompt);

        try
        {
            var response = await _chat.GetChatMessageContentsAsync(
                history,
                CreateSettings(),
                cancellationToken: ct);

            var raw = response.FirstOrDefault()?.Content;
            var completionLength = raw?.Length ?? 0;

            if (!_jsonParser.TryParse(raw, out var ai) || ai is null)
            {
                throw new ModerationProviderException(
                    ModerationFailureKind.InvalidJson,
                    "The moderation AI response could not be parsed as JSON.",
                    isTransient: true);
            }

            var analysis = BuildAnalysis(ai, completionLength);
            return ModerationResult.FromAnalysis(analysis);
        }
        catch (ModerationProviderException)
        {
            throw;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw MapHttpFailure(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure during the moderation AI call.");
            throw new ModerationProviderException(
                ModerationFailureKind.Internal,
                "The moderation AI call failed unexpectedly.",
                isTransient: true,
                ex);
        }
    }

    private ModerationAnalysis BuildAnalysis(ModerationAiResponse ai, int completionLength)
    {
        var riskScore = Math.Round(ai.RiskScore is double risk ? Math.Clamp(risk, 0, 100) : DeriveRiskScore(ai));
        var confidence = ai.Confidence is double conf ? Math.Clamp(conf, 0, 1) : 0.5;
        var riskLevel = ParseRiskLevel(ai.RiskLevel) ?? MapRiskLevel(riskScore);
        var action = ParseAction(ai.Action) ?? MapAction(riskScore);
        var isSafe = riskScore <= 20;
        var reason = string.IsNullOrWhiteSpace(ai.Reason) ? BuildDefaultReason(action, riskScore) : ai.Reason;

        var categories = ai.Categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .Select(c => new ModerationCategoryScore(c.Category, Math.Clamp(c.Score, 0, 1)))
            .ToList();

        _logger.LogInformation(
            "Moderation AI completed. RiskScore={RiskScore} Confidence={Confidence} Action={Action} " +
            "RiskLevel={RiskLevel} CategoryCount={CategoryCount} CompletionLength={CompletionLength}",
            riskScore, confidence, action, riskLevel, categories.Count, completionLength);

        return new ModerationAnalysis(
            isSafe,
            riskScore,
            confidence,
            riskLevel,
            action,
            reason,
            categories,
            _options.PromptVersion,
            _options.ModelId);
    }

    private static double DeriveRiskScore(ModerationAiResponse ai)
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
            var maxScore = ai.Categories.Max(c => Math.Clamp(c.Score, 0, 1));
            if (maxScore > 0)
                return maxScore * 100;
        }

        return 10;
    }

    private static RiskLevel? ParseRiskLevel(string? value)
        => Enum.TryParse<RiskLevel>(value, ignoreCase: true, out var level) ? level : null;

    private static ModerationAction? ParseAction(string? value)
        => Enum.TryParse<ModerationAction>(value, ignoreCase: true, out var action) ? action : null;

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

    private static ModerationProviderException MapHttpFailure(HttpRequestException ex)
    {
        var kind = ex.StatusCode switch
        {
            HttpStatusCode.BadRequest => ModerationFailureKind.BadRequest,
            HttpStatusCode.Unauthorized => ModerationFailureKind.Unauthorized,
            HttpStatusCode.Forbidden => ModerationFailureKind.Forbidden,
            HttpStatusCode.RequestTimeout => ModerationFailureKind.Timeout,
            HttpStatusCode.TooManyRequests => ModerationFailureKind.Network,
            null => ModerationFailureKind.Network,
            _ when (int)ex.StatusCode >= 500 => ModerationFailureKind.Internal,
            _ => ModerationFailureKind.Network
        };

        var isTransient = kind is ModerationFailureKind.Timeout
            or ModerationFailureKind.Network
            or ModerationFailureKind.Internal;

        return new ModerationProviderException(
            kind,
            "The moderation AI service rejected or failed the request.",
            isTransient,
            ex);
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
