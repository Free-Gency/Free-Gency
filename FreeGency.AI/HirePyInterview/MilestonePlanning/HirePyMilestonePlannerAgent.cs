using System.Text.Json;
using System.Text.Json.Serialization;
using FreeGency.AI.Common;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.HirePyInterview.MilestonePlanning;

public sealed class HirePyMilestonePlannerAgent : IHirePyMilestonePlannerAgent
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
    private const int MaxMilestoneCount = 20;
    private const int MaxDefinitionOfDoneLength = 4000;
    private const int MaxTitleLength = 120;

    public HirePyMilestonePlannerAgent(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<MilestonePlanningReply> PlanNextStepAsync(
        MilestonePlanningContext context,
        CancellationToken cancellationToken = default)
    {
        var history = context.History.TakeLast(MaxHistoryEntries).ToList();
        var historyBlock = history.Count == 0
            ? "(no prior messages)"
            : string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));

        var userPayload = $"""
            {PromptInputSanitizer.Section("CANDIDATE", context.CandidateName)}

            {PromptInputSanitizer.Section("PROJECT BRIEF", context.ProjectBrief)}

            {PromptInputSanitizer.Section("CANDIDATE PROPOSAL", context.ProposalSummary)}

            {PromptInputSanitizer.Section("FINAL ATTEMPT", context.IsFinalAttempt ? "yes - finalize the best plan now, even if not perfect." : "no")}

            {PromptInputSanitizer.Section("CONVERSATION HISTORY", historyBlock)}
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.HirePyMilestonePlanner);
        chatHistory.AddUserMessage(userPayload);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));

        var response = await _chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: timeout.Token);
        var raw = response.FirstOrDefault()?.Content ?? string.Empty;
        return ParseModelOutput(raw);
    }

    internal static MilestonePlanningReply ParseModelOutput(string raw)
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
            return new MilestonePlanningReply
            {
                Message = AiOutputParsing.SanitizeMessage(
                    AiOutputParsing.Truncate(AiOutputParsing.UnescapeJsonString(messageOnly), MaxReplyLength)),
                Outcome = MilestonePlanningOutcome.NeedsRevision
            };
        }

        var plain = cleaned.Trim();
        if (plain.Length > 0 && !plain.StartsWith('{'))
        {
            return new MilestonePlanningReply
            {
                Message = AiOutputParsing.SanitizeMessage(AiOutputParsing.Truncate(plain, MaxReplyLength)),
                Outcome = MilestonePlanningOutcome.NeedsRevision
            };
        }

        return new MilestonePlanningReply
        {
            Message = "Could you share the milestone plan (phases, deliverables, durations and costs)?",
            Outcome = MilestonePlanningOutcome.NeedsRevision
        };
    }

    private static MilestonePlanningReply Normalize(PlannerOutput parsed)
    {
        var message = string.IsNullOrWhiteSpace(parsed.Message)
            ? "Could you adjust the milestone plan so it matches the project requirements and budget?"
            : parsed.Message;

        var outcome = ResolveOutcome(parsed.Outcome);

        var milestones = new List<ProposedMilestone>();
        if (outcome == MilestonePlanningOutcome.Finalize && parsed.Milestones is not null)
        {
            var order = 0;
            foreach (var item in parsed.Milestones.Take(MaxMilestoneCount))
            {
                if (string.IsNullOrWhiteSpace(item.Title) || (item.Cost ?? 0) <= 0)
                    continue;

                order++;
                milestones.Add(new ProposedMilestone
                {
                    Title = AiOutputParsing.SanitizeMessage(
                        AiOutputParsing.Truncate(item.Title.Trim(), MaxTitleLength)),
                    DefinitionOfDone = ComposeDefinitionOfDone(item),
                    Amount = item.Cost!.Value,
                    DueDate = ResolveDueDate(item.DurationDays),
                    SortOrder = order
                });
            }
        }

        return new MilestonePlanningReply
        {
            Message = AiOutputParsing.SanitizeMessage(AiOutputParsing.Truncate(message, MaxReplyLength)),
            Outcome = outcome,
            Issues = parsed.Issues ?? [],
            Milestones = milestones
        };
    }

    private static MilestonePlanningOutcome ResolveOutcome(string? outcome)
    {
        if (string.IsNullOrWhiteSpace(outcome))
            return MilestonePlanningOutcome.NeedsRevision;

        var value = outcome.Trim().Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
        return value.Contains("final", StringComparison.Ordinal)
               || value.Contains("accept", StringComparison.Ordinal)
               || value.Contains("ready", StringComparison.Ordinal)
               || value is "done" or "ok"
            ? MilestonePlanningOutcome.Finalize
            : MilestonePlanningOutcome.NeedsRevision;
    }

    private static string ComposeDefinitionOfDone(MilestoneItemOutput item)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.Description))
            parts.Add(item.Description.Trim());

        if (item.Deliverables is { Count: > 0 })
        {
            parts.Add("Deliverables:");
            parts.AddRange(item.Deliverables
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => $"  - {d.Trim()}"));
        }

        if (item.Dependencies is { Count: > 0 })
        {
            parts.Add("Dependencies:");
            parts.AddRange(item.Dependencies
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => $"  - {d.Trim()}"));
        }

        if (item.AcceptanceCriteria is { Count: > 0 })
        {
            parts.Add("Acceptance criteria:");
            parts.AddRange(item.AcceptanceCriteria
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => $"  - {a.Trim()}"));
        }

        var text = AiOutputParsing.SanitizeMessage(string.Join("\n", parts));
        return text.Length <= MaxDefinitionOfDoneLength ? text : text[..MaxDefinitionOfDoneLength].TrimEnd(' ', ',', '.') + "…";
    }

    private static DateTime? ResolveDueDate(int? durationDays)
    {
        if (durationDays is null or <= 0)
            return null;

        return DateTime.UtcNow.Date.AddDays(durationDays.Value);
    }

    private static bool TryDeserialize(string json, out PlannerOutput? result)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                result = null;
                return false;
            }

            var parsed = JsonSerializer.Deserialize<PlannerOutput>(json, JsonOpts);
            if (parsed is null || string.IsNullOrWhiteSpace(parsed.Message))
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

    private sealed class PlannerOutput
    {
        public string? Message { get; set; }
        public string? Outcome { get; set; }
        public IReadOnlyList<string>? Issues { get; set; }
        public IReadOnlyList<MilestoneItemOutput>? Milestones { get; set; }
    }

    private sealed class MilestoneItemOutput
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IReadOnlyList<string>? Deliverables { get; set; }
        public IReadOnlyList<string>? AcceptanceCriteria { get; set; }
        public IReadOnlyList<string>? Dependencies { get; set; }
        public int? DurationDays { get; set; }
        public decimal? Cost { get; set; }
    }
}
