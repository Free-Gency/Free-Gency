using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.MilestonePlanAssist;

public enum MilestonePlanAssistMode
{
    FullPlan = 0,
    ApplyChangeRequest = 1,
    Milestone = 2,
    Field = 3
}

public sealed class MilestonePlanAssistDraftItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("definitionOfDone")]
    public string DefinitionOfDone { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("dueDate")]
    public string? DueDate { get; set; }
}

public sealed class MilestonePlanAssistResult
{
    public List<MilestonePlanAssistDraftItem> Milestones { get; init; } = [];
}

public sealed class MilestonePlanAssistChatService
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public MilestonePlanAssistChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<MilestonePlanAssistResult> AssistAsync(
        string projectContext,
        MilestonePlanAssistMode mode,
        IReadOnlyList<MilestonePlanAssistDraftItem> currentMilestones,
        int? milestoneIndex,
        string? field,
        string? changeComment,
        CancellationToken ct = default)
    {
        var variation = BuildVariationDirective(mode);
        var task = BuildTask(mode, currentMilestones, milestoneIndex, field, changeComment);
        var payload = $"""
            PROJECT CONTEXT:
            {projectContext}

            CURRENT DRAFT MILESTONES (JSON):
            {JsonSerializer.Serialize(currentMilestones, JsonOpts)}

            {variation}

            TASK:
            {task}
            """;

        var history = new ChatHistory();
        history.AddSystemMessage(PromptTemplates.MilestonePlanAssist);
        history.AddUserMessage(payload);

        var response = await _chat.GetChatMessageContentsAsync(history, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? "{}";
        return Parse(raw, mode, currentMilestones, milestoneIndex, field);
    }

    private static string BuildVariationDirective(MilestonePlanAssistMode mode)
    {
        if (mode is MilestonePlanAssistMode.Field or MilestonePlanAssistMode.ApplyChangeRequest)
            return "VARIATION DIRECTIVE: keep continuity with the requested edit; no forced redesign.";

        var styles = new[]
        {
            "discovery-first then build",
            "MVP slice then polish",
            "design-system then feature verticals",
            "backend contracts first, UI after",
            "feature-by-feature vertical slices",
            "riskiest integration first",
            "client-visible demo every milestone",
            "quality gates heavy (QA/UAT splits)",
            "content/ops readiness included",
            "performance & hardening as its own late milestone"
        };
        var emphases = new[]
        {
            "strong acceptance tests in every DoD",
            "handoff artifacts (docs, Figma, API specs)",
            "security & auth readiness",
            "responsive UI + accessibility checks",
            "data model & migrations clarity",
            "admin/ops tooling",
            "integrations & webhooks",
            "analytics & monitoring hooks",
            "launch checklist & rollback notes",
            "stakeholder review checkpoints"
        };
        var budgetShapes = new[]
        {
            "front-load payment on early discovery/design",
            "even split across milestones",
            "back-load payment on delivery/UAT",
            "slightly larger middle build milestones",
            "small kickoff + larger delivery milestones"
        };

        var seed = Guid.NewGuid().ToString("N")[..12];
        var count = Random.Shared.Next(3, 7); // 3..6
        var style = styles[Random.Shared.Next(styles.Length)];
        var emphasis = emphases[Random.Shared.Next(emphases.Length)];
        var budgetShape = budgetShapes[Random.Shared.Next(budgetShapes.Length)];
        var twist = Random.Shared.Next(4) switch
        {
            0 => "Split what others might combine into two sharper milestones.",
            1 => "Combine related work that others might over-split.",
            2 => "Add one milestone focused on validation/demo with the client.",
            _ => "Name milestones after concrete deliverables, not department labels."
        };

        return $"""
            VARIATION DIRECTIVE (mandatory — make this plan distinct):
            - VariationSeed: {seed}
            - PreferredMilestoneCount: {count}
            - PlanningStyle: {style}
            - Emphasis: {emphasis}
            - BudgetShape: {budgetShape}
            - CreativeTwist: {twist}
            - Do NOT reuse a previous generic template. Titles and DoD must read differently from a stock plan.
            - Reallocate amounts meaningfully while keeping the plan total within budgetMax.
            """;
    }

    private static string BuildTask(
        MilestonePlanAssistMode mode,
        IReadOnlyList<MilestonePlanAssistDraftItem> current,
        int? milestoneIndex,
        string? field,
        string? changeComment)
    {
        const string fullShape =
            """Return JSON: {"milestones":[{"title":"","definitionOfDone":"","amount":0,"dueDate":"YYYY-MM-DD"}]}""";

        return mode switch
        {
            MilestonePlanAssistMode.FullPlan =>
                "Generate a complete milestone plan for this project from scratch.\n"
                + "Follow the VARIATION DIRECTIVE so this plan is unique vs other freelancers on the same project.\n"
                + "If CURRENT DRAFT is non-empty, intentionally redesign it (different structure/wording/split), not a light rewrite.\n"
                + fullShape,

            MilestonePlanAssistMode.ApplyChangeRequest =>
                "Revise the CURRENT DRAFT to satisfy this change request.\n"
                + "CHANGE REQUEST:\n"
                + (changeComment ?? "(none)")
                + "\nKeep what still works; update titles, DoD, amounts, and due dates as needed.\n"
                + "Return the FULL revised list.\n"
                + fullShape,

            MilestonePlanAssistMode.Milestone =>
                $"Regenerate ONLY milestone index {milestoneIndex ?? 0} (0-based) as a complete item.\n"
                + "Keep sibling milestones coherent (budget remainder, sequential dates).\n"
                + "Make this regenerated milestone feel freshly proposed — different title phrasing and richer DoD — while staying in scope.\n"
                + "Return JSON with a single-item milestones array for that milestone only.\n"
                + fullShape,

            MilestonePlanAssistMode.Field =>
                $"Improve ONLY the \"{field}\" value of milestone index {milestoneIndex ?? 0} (0-based).\n"
                + "Keep the same meaning and intent, but make it clearer, more professional, and more concrete.\n"
                + "If the current value is empty or placeholder-like, write a solid draft from project context and sibling milestones.\n"
                + "Do NOT rewrite other fields. Do NOT invent a totally different scope.\n"
                + "Return JSON with that one milestone (all fields present; unchanged fields must match current).\n"
                + fullShape
                + "\nCurrent field value to improve:\n"
                + "\"\"\"\n"
                + (milestoneIndex is int i && i >= 0 && i < current.Count
                    ? (string.Equals(field, "definitionOfDone", StringComparison.OrdinalIgnoreCase)
                        ? (string.IsNullOrWhiteSpace(current[i].DefinitionOfDone) ? "(empty)" : current[i].DefinitionOfDone)
                        : (string.IsNullOrWhiteSpace(current[i].Title) ? "(empty)" : current[i].Title))
                    : "(empty)")
                + "\n\"\"\"",

            _ => "Return {\"milestones\":[]}"
        };
    }

    internal static MilestonePlanAssistResult Parse(
        string raw,
        MilestonePlanAssistMode mode,
        IReadOnlyList<MilestonePlanAssistDraftItem> current,
        int? milestoneIndex,
        string? field)
    {
        var cleaned = StripJsonFences(raw);
        var json = ExtractJsonObject(cleaned) ?? cleaned;

        List<MilestonePlanAssistDraftItem> items = [];
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            JsonElement array = default;
            var found = false;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("milestones", out var m)
                && m.ValueKind == JsonValueKind.Array)
            {
                array = m;
                found = true;
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                array = root;
                found = true;
            }

            if (found)
            {
                foreach (var el in array.EnumerateArray())
                {
                    items.Add(new MilestonePlanAssistDraftItem
                    {
                        Title = el.TryGetProperty("title", out var t) ? t.GetString()?.Trim() ?? "" : "",
                        DefinitionOfDone = el.TryGetProperty("definitionOfDone", out var d)
                            ? d.GetString()?.Trim() ?? ""
                            : "",
                        Amount = ReadDecimal(el, "amount"),
                        DueDate = NormalizeDueDate(
                            el.TryGetProperty("dueDate", out var dd) ? dd.GetString() : null)
                    });
                }
            }
        }
        catch (JsonException)
        {
            items = [];
        }

        items = items
            .Where(i => !string.IsNullOrWhiteSpace(i.Title) || i.Amount > 0 || !string.IsNullOrWhiteSpace(i.DefinitionOfDone))
            .ToList();

        if (mode is MilestonePlanAssistMode.Milestone or MilestonePlanAssistMode.Field)
        {
            if (items.Count == 0)
                return new MilestonePlanAssistResult { Milestones = [] };

            var generated = items[0];
            if (mode == MilestonePlanAssistMode.Field
                && milestoneIndex is int idx
                && idx >= 0
                && idx < current.Count)
            {
                var baseRow = current[idx];
                if (string.Equals(field, "definitionOfDone", StringComparison.OrdinalIgnoreCase))
                {
                    generated = new MilestonePlanAssistDraftItem
                    {
                        Title = baseRow.Title,
                        DefinitionOfDone = string.IsNullOrWhiteSpace(generated.DefinitionOfDone)
                            ? baseRow.DefinitionOfDone
                            : generated.DefinitionOfDone,
                        Amount = baseRow.Amount,
                        DueDate = baseRow.DueDate
                    };
                }
                else
                {
                    generated = new MilestonePlanAssistDraftItem
                    {
                        Title = string.IsNullOrWhiteSpace(generated.Title) ? baseRow.Title : generated.Title,
                        DefinitionOfDone = baseRow.DefinitionOfDone,
                        Amount = baseRow.Amount,
                        DueDate = baseRow.DueDate
                    };
                }
            }

            return new MilestonePlanAssistResult { Milestones = [generated] };
        }

        return new MilestonePlanAssistResult { Milestones = items };
    }

    private static decimal ReadDecimal(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return 0;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
        if (p.ValueKind == JsonValueKind.String
            && decimal.TryParse(p.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return 0;
    }

    private static string? NormalizeDueDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var text = raw.Trim();
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}"))
            return text[..10];
        return null;
    }

    private static string StripJsonFences(string raw)
    {
        var text = raw.Trim();
        if (!text.StartsWith("```", StringComparison.Ordinal))
            return text;

        var firstNewline = text.IndexOf('\n');
        if (firstNewline != -1)
            text = text[(firstNewline + 1)..];

        var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
        if (lastFence != -1)
            text = text[..lastFence];

        return text.Trim();
    }

    private static string? ExtractJsonObject(string text)
    {
        var objStart = text.IndexOf('{');
        var arrStart = text.IndexOf('[');
        if (objStart < 0 && arrStart < 0) return null;

        var start = objStart < 0 ? arrStart
            : arrStart < 0 ? objStart
            : Math.Min(objStart, arrStart);
        var open = text[start];
        var close = open == '{' ? '}' : ']';
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];
            if (inString)
            {
                if (escape) { escape = false; continue; }
                if (c == '\\') { escape = true; continue; }
                if (c == '"') inString = false;
                continue;
            }

            if (c == '"') { inString = true; continue; }
            if (c == open) depth++;
            else if (c == close)
            {
                depth--;
                if (depth == 0)
                    return text[start..(i + 1)];
            }
        }

        return null;
    }
}
