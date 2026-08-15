using System.Globalization;
using System.Text.Json;
using FreeGency.Domain.Constants;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.ProjectDrafting;

public class ProjectGenerationService : IProjectGenerationService
{
    private readonly ProjectDraftService _draftService;
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private static readonly string[] SupportedComplexities = ["Low", "Medium", "High"];

    public ProjectGenerationService(ProjectDraftService draftService, IChatCompletionService chat)
    {
        _draftService = draftService;
        _chat = chat;
    }

    public async Task<GeneratedProjectDraft> GenerateAsync(string userInput, CancellationToken ct = default)
    {
        var draft = await _draftService.GenerateDraftAsync(userInput);

        var metaJson = await AskAsync(BuildMetaPrompt(draft, userInput), ct);
        var meta = DeserializeMeta(metaJson);

        var budget = ClampBudget(meta.BudgetMin, meta.BudgetMax);

        return new GeneratedProjectDraft
        {
            Title = draft.Title,
            Description = draft.Description,
            CategoryId = draft.CategoryId,
            CategoryName = draft.CategoryName,
            NeedsManualCategoryReview = draft.NeedsManualCategoryReview,
            SpecialtyIds = draft.SpecialtyIds,
            SpecialtyNames = draft.SpecialtyNames,
            SkillIds = draft.SkillIds,
            SkillNames = draft.SkillNames,
            IsFixedPrice = meta.IsFixedPrice,
            BudgetMin = budget.min,
            BudgetMax = budget.max,
            Currency = NormalizeCurrency(meta.Currency),
            EstimatedDurationDays = NormalizeDuration(meta.EstimatedDurationDays),
            Deadline = ParseDeadline(meta.Deadline),
            Complexity = NormalizeComplexity(meta.Complexity),
            Requirements = Clean(meta.Requirements),
            Features = Clean(meta.Features),
            Risks = Clean(meta.Risks),
        };
    }

    private static string BuildMetaPrompt(ProjectDraftResponse draft, string userInput)
    {
        var skillsBlock = draft.SkillNames.Count > 0
            ? string.Join(", ", draft.SkillNames)
            : "(no specific skills resolved yet)";

        return $$"""
            You are a freelance hiring planner. Finalize a job post drafted from a client idea.
            Drafted so far:
            - Title: "{{draft.Title}}"
            - Category: "{{draft.CategoryName}}"
            - Specialties: "{{string.Join(", ", draft.SpecialtyNames)}}"
            - Skills: "{{skillsBlock}}"

            Propose the commercial plan:
            - isFixedPrice: true if the budget is a fixed scope price, false if it should be hourly/bid-based.
            - budgetMin, budgetMax: positive numbers in the SAME currency, budgetMax >= budgetMin >= 0.
            - currency: one of exactly: USD, EUR, GBP, EGP, SAR, AED, KWD, QAR, BHD, OMR, JOD.
            - estimatedDurationDays: a positive integer number of calendar days.
            - deadline: an ISO 8601 date string (e.g. "2026-12-31") or null if unknown.
            - complexity: one of exactly: Low, Medium, High.
            - requirements: 3-6 concise functional requirements.
            - features: 3-6 concise key features.
            - risks: 1-4 concise likely risks.

            Respond ONLY as JSON:
            {"isFixedPrice": true, "budgetMin": 0, "budgetMax": 0, "currency": "USD",
             "estimatedDurationDays": 30, "deadline": null, "complexity": "Medium",
             "requirements": [], "features": [], "risks": []}

            Client input: {{userInput}}
            """;
    }

    private static (decimal min, decimal max) ClampBudget(decimal? min, decimal? max)
    {
        var lo = Math.Max(0, min ?? 0);
        var hi = Math.Max(lo, max ?? lo);
        return (lo, hi);
    }

    private static string NormalizeCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return Currencies.USD;

        return Currencies.Supported.FirstOrDefault(c =>
            string.Equals(c, currency, StringComparison.OrdinalIgnoreCase)) ?? Currencies.USD;
    }

    private static int? NormalizeDuration(int? days)
        => days is > 0 ? days : null;

    private static DateTime? ParseDeadline(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            return parsed;

        return null;
    }

    private static string? NormalizeComplexity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return SupportedComplexities.FirstOrDefault(c =>
            string.Equals(c, value, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> Clean(List<string>? items)
        => items?
            .Select(i => i.Trim())
            .Where(i => i.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList() ?? [];

    private ProjectMetaResult DeserializeMeta(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ProjectMetaResult>(json, JsonOpts)
                   ?? throw new JsonException("Project plan meta was null.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("The AI returned a malformed project plan.");
        }
    }

    private async Task<string> AskAsync(string prompt, CancellationToken ct)
    {
        var history = new ChatHistory();
        history.AddUserMessage(prompt);
        var result = await _chat.GetChatMessageContentsAsync(history, cancellationToken: ct);
        var raw = result[0].Content ?? "{}";
        return StripJsonFences(raw);
    }

    private static string StripJsonFences(string raw)
    {
        var text = raw.Trim();

        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline != -1)
                text = text[(firstNewline + 1)..];

            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence != -1)
                text = text[..lastFence];
        }

        return text.Trim();
    }

    private sealed class ProjectMetaResult
    {
        public bool IsFixedPrice { get; set; } = true;
        public decimal? BudgetMin { get; set; }
        public decimal? BudgetMax { get; set; }
        public string? Currency { get; set; }
        public int? EstimatedDurationDays { get; set; }
        public string? Deadline { get; set; }
        public string? Complexity { get; set; }
        public List<string>? Requirements { get; set; }
        public List<string>? Features { get; set; }
        public List<string>? Risks { get; set; }
    }
}
