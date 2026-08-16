using System.Text.Json;
using System.Text.RegularExpressions;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.HiringAgent;

public sealed class DiscussionRankingChatService
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public DiscussionRankingChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<DiscussionRankingResult> RankAsync(
        string projectContext,
        string candidatesBlock,
        CancellationToken ct = default)
    {
        var userPayload = $"""
            PROJECT:
            {projectContext}

            CANDIDATES & DISCUSSIONS:
            {candidatesBlock}
            """;

        var history = new ChatHistory();
        history.AddSystemMessage(PromptTemplates.HiringDiscussionRanking);
        history.AddUserMessage(userPayload);

        var response = await _chat.GetChatMessageContentsAsync(history, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? "{}";
        return Parse(raw);
    }

    internal static DiscussionRankingResult Parse(string raw)
    {
        try
        {
            var cleaned = StripJsonFences(raw);
            var json = ExtractJsonObject(cleaned) ?? cleaned;
            var parsed = JsonSerializer.Deserialize<DiscussionRankingResultDto>(json, JsonOpts);
            if (parsed is null)
                return Empty("Unable to parse ranking output.");

            return new DiscussionRankingResult
            {
                OverallSummary = parsed.OverallSummary?.Trim() ?? string.Empty,
                Risks = parsed.Risks ?? [],
                Ranked = (parsed.Ranked ?? [])
                    .Select(r => new RankedDiscussionItem
                    {
                        CandidateId = r.CandidateId ?? string.Empty,
                        Score = r.Score,
                        Summary = r.Summary ?? string.Empty,
                        Strengths = r.Strengths ?? [],
                        Weaknesses = r.Weaknesses ?? []
                    })
                    .Where(r => !string.IsNullOrWhiteSpace(r.CandidateId))
                    .ToList()
            };
        }
        catch
        {
            return Empty("Ranking failed; please review discussions manually.");
        }
    }

    private static DiscussionRankingResult Empty(string summary) => new()
    {
        OverallSummary = summary,
        Risks = [],
        Ranked = []
    };

    private sealed class DiscussionRankingResultDto
    {
        public string? OverallSummary { get; set; }
        public List<string>? Risks { get; set; }
        public List<RankedDiscussionItemDto>? Ranked { get; set; }
    }

    private sealed class RankedDiscussionItemDto
    {
        public string? CandidateId { get; set; }
        public float Score { get; set; }
        public string? Summary { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? Weaknesses { get; set; }
    }

    private static string StripJsonFences(string raw)
    {
        var m = Regex.Match(raw, @"```(?:json)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim() : raw.Trim();
    }

    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        return text[start..(end + 1)];
    }
}
