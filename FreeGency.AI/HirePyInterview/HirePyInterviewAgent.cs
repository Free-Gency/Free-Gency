using System.Text.Json;
using System.Text.Json.Serialization;
using FreeGency.AI.Common;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.HirePyInterview;

public sealed class HirePyInterviewAgent : IHirePyInterviewAgent
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private const int MaxHistoryEntries = 10;
    private const int MaxReplyLength = 800;

    public HirePyInterviewAgent(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<HirePyInterviewReply> GetNextReplyAsync(HirePyInterviewContext context, CancellationToken ct = default)
    {
        var history = context.History.TakeLast(MaxHistoryEntries).ToList();
        var historyBlock = history.Count == 0
            ? "(no prior messages — this is the opening message)"
            : string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));

        var userPayload = $"""
            {PromptInputSanitizer.Section("CANDIDATE", context.CandidateName)}

            {PromptInputSanitizer.Section("PROJECT BRIEF", context.ProjectBrief)}

            {PromptInputSanitizer.Section("CANDIDATE PROPOSAL", context.ProposalSummary)}

            {PromptInputSanitizer.Section("CONVERSATION HISTORY", historyBlock)}
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.HirePyInterviewer);
        chatHistory.AddUserMessage(userPayload);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));

        var response = await _chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: timeout.Token);
        var raw = response.FirstOrDefault()?.Content ?? string.Empty;
        return ParseModelOutput(raw);
    }

    internal static HirePyInterviewReply ParseModelOutput(string raw)
    {
        var cleaned = AiOutputParsing.StripJsonFences(raw);

        if (TryDeserialize(cleaned, out var parsed))
            return Normalize(parsed);

        var repaired = AiOutputParsing.RepairJsonStrings(cleaned);
        if (TryDeserialize(repaired, out parsed))
            return Normalize(parsed);

        var messageOnly = AiOutputParsing.ExtractMessageField(cleaned) ?? AiOutputParsing.ExtractMessageField(raw);
        if (!string.IsNullOrWhiteSpace(messageOnly))
        {
            return new HirePyInterviewReply
            {
                Message = AiOutputParsing.SanitizeMessage(
                    AiOutputParsing.Truncate(AiOutputParsing.UnescapeJsonString(messageOnly), MaxReplyLength)),
                Decision = HirePyInterviewDecision.AskQuestion
            };
        }

        var plain = cleaned.Trim();
        if (plain.Length > 0 && !plain.StartsWith('{'))
        {
            return new HirePyInterviewReply
            {
                Message = AiOutputParsing.SanitizeMessage(AiOutputParsing.Truncate(plain, MaxReplyLength)),
                Decision = HirePyInterviewDecision.AskQuestion
            };
        }

        return new HirePyInterviewReply
        {
            Message = "Could you tell me a bit more about your approach for this project?",
            Decision = HirePyInterviewDecision.AskQuestion
        };
    }

    private static HirePyInterviewReply Normalize(HirePyInterviewReply parsed)
    {
        if (string.IsNullOrWhiteSpace(parsed.Message))
            parsed.Message = "Thanks — could you share more detail on how you'd tackle this project?";

        parsed.Message = AiOutputParsing.SanitizeMessage(
            AiOutputParsing.Truncate(parsed.Message, MaxReplyLength));
        parsed.Decision = parsed.Decision switch
        {
            HirePyInterviewDecision.RequestMilestonePlan => HirePyInterviewDecision.RequestMilestonePlan,
            _ => HirePyInterviewDecision.AskQuestion
        };
        return parsed;
    }

    internal static string SanitizeMessage(string message)
        => AiOutputParsing.SanitizeMessage(message);

    private static bool TryDeserialize(string json, out HirePyInterviewReply? result)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                result = null;
                return false;
            }

            var parsed = JsonSerializer.Deserialize<ModelOutput>(json, JsonOpts);
            if (parsed is null || string.IsNullOrWhiteSpace(parsed.Message))
            {
                result = null;
                return false;
            }

            result = new HirePyInterviewReply
            {
                Message = parsed.Message,
                Decision = ResolveDecision(parsed.Decision)
            };
            return true;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }

    private static HirePyInterviewDecision ResolveDecision(string? decision)
    {
        if (string.IsNullOrWhiteSpace(decision))
            return HirePyInterviewDecision.AskQuestion;

        var value = decision.Trim().Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
        return value.Contains("milestone", StringComparison.Ordinal) || value is "request" or "complete" or "done" or "conclude"
            ? HirePyInterviewDecision.RequestMilestonePlan
            : HirePyInterviewDecision.AskQuestion;
    }

    private sealed class ModelOutput
    {
        public string? Message { get; set; }
        public string? Decision { get; set; }
    }
}
