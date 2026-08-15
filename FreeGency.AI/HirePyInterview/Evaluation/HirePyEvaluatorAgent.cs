using System.Text.Json;
using System.Text.Json.Serialization;
using FreeGency.AI.Common;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.HirePyInterview.Evaluation;

public sealed class HirePyEvaluatorAgent : IHirePyEvaluatorAgent
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private const int MaxDiscussionEntries = 10;
    private const int MaxListItems = 10;
    private const int MaxItemLength = 300;
    private const int MaxReasonLength = 1000;

    public HirePyEvaluatorAgent(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<HirePyEvaluationReply> EvaluateAsync(
        HirePyEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var discussion = Truncate(context.DiscussionSummary, MaxDiscussionEntries * 400);

        var userPayload = $"""
            {PromptInputSanitizer.Section("CANDIDATE", context.CandidateName)}

            {PromptInputSanitizer.Section("PROJECT BRIEF", context.ProjectBrief)}

            {PromptInputSanitizer.Section("CANDIDATE PROPOSAL", context.ProposalSummary)}

            {PromptInputSanitizer.Section("FINALIZED MILESTONE PLAN", context.MilestonePlanSummary)}

            {PromptInputSanitizer.Section("ORIGINAL RANKING", $"position {context.RankingPosition}, score {context.RankingScore}")}

            {PromptInputSanitizer.Section("PRIVATE DISCUSSION TRANSCRIPT", discussion)}
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.HirePyEvaluator);
        chatHistory.AddUserMessage(userPayload);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));

        var response = await _chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: timeout.Token);
        var raw = response.FirstOrDefault()?.Content ?? string.Empty;
        return ParseModelOutput(raw);
    }

    internal static HirePyEvaluationReply ParseModelOutput(string raw)
    {
        var cleaned = AiOutputParsing.StripJsonFences(raw);

        if (!TryDeserialize(cleaned, out var parsed))
        {
            var repaired = AiOutputParsing.RepairJsonStrings(cleaned);
            TryDeserialize(repaired, out parsed);
        }

        if (parsed is null)
            return Invalid();

        return Normalize(parsed);
    }

    private static HirePyEvaluationReply Normalize(EvaluationOutput parsed)
    {
        var technical = NormalizeScore(parsed.TechnicalScore);
        var requirements = NormalizeScore(parsed.RequirementsScore);
        var architecture = NormalizeScore(parsed.ArchitectureScore);
        var implementation = NormalizeScore(parsed.ImplementationScore);
        var milestone = NormalizeScore(parsed.MilestoneScore);
        var timeline = NormalizeScore(parsed.TimelineScore);
        var budget = NormalizeScore(parsed.BudgetScore);
        var communication = NormalizeScore(parsed.CommunicationScore);
        var risk = NormalizeScore(parsed.RiskScore);
        var overall = NormalizeScore(parsed.OverallScore);

        // Every spec-required score must be present and numeric; the reason must exist.
        if (technical is null || requirements is null || milestone is null || timeline is null
            || budget is null || communication is null || risk is null || overall is null
            || string.IsNullOrWhiteSpace(parsed.Reason))
            return Invalid();

        return new HirePyEvaluationReply
        {
            IsValid = true,
            TechnicalScore = technical.Value,
            RequirementsScore = requirements.Value,
            ArchitectureScore = architecture ?? technical.Value,
            ImplementationScore = implementation ?? technical.Value,
            MilestoneScore = milestone.Value,
            TimelineScore = timeline.Value,
            BudgetScore = budget.Value,
            CommunicationScore = communication.Value,
            RiskScore = risk.Value,
            OverallScore = overall.Value,
            Strengths = NormalizeList(parsed.Strengths),
            Concerns = NormalizeList(parsed.Concerns),
            Risks = NormalizeList(parsed.Risks),
            Reason = NormalizeReason(parsed.Reason)
        };
    }

    private static string NormalizeReason(string reason)
    {
        var text = AiOutputParsing.SanitizeMessage(reason.Trim());
        return text.Length <= MaxReasonLength ? text : text[..MaxReasonLength].TrimEnd(' ', ',', '.') + "…";
    }

    private static List<string> NormalizeList(IReadOnlyList<string>? items)
    {
        if (items is null)
            return [];

        var result = new List<string>();
        foreach (var item in items.Take(MaxListItems))
        {
            var text = AiOutputParsing.SanitizeMessage(item);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            text = text.Length <= MaxItemLength ? text : text[..MaxItemLength].TrimEnd(' ', ',', '.');
            if (text.Length > 0)
                result.Add(text);
        }

        return result;
    }

    private static int? NormalizeScore(double? score)
    {
        if (score is null || double.IsNaN(score.Value) || double.IsInfinity(score.Value))
            return null;

        return (int)Math.Clamp(Math.Round(score.Value, 0, MidpointRounding.AwayFromZero), 0, 100);
    }

    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "(no prior discussion)";

        return text.Length <= maxLength ? text : text[..maxLength].TrimEnd(' ', ',', '.') + "…";
    }

    private static HirePyEvaluationReply Invalid()
        => new() { IsValid = false };

    private static bool TryDeserialize(string json, out EvaluationOutput? result)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                result = null;
                return false;
            }

            var parsed = JsonSerializer.Deserialize<EvaluationOutput>(json, JsonOpts);
            if (parsed is null)
            {
                result = null;
                return false;
            }

            result = parsed;
            return true;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }

    private sealed class EvaluationOutput
    {
        public double? TechnicalScore { get; set; }
        public double? RequirementsScore { get; set; }
        public double? ArchitectureScore { get; set; }
        public double? ImplementationScore { get; set; }
        public double? MilestoneScore { get; set; }
        public double? TimelineScore { get; set; }
        public double? BudgetScore { get; set; }
        public double? CommunicationScore { get; set; }
        public double? RiskScore { get; set; }
        public double? OverallScore { get; set; }
        public IReadOnlyList<string>? Strengths { get; set; }
        public IReadOnlyList<string>? Concerns { get; set; }
        public IReadOnlyList<string>? Risks { get; set; }
        public string? Reason { get; set; }
    }
}
