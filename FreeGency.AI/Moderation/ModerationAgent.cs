using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FreeGency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.Moderation;

public sealed class ModerationAgent : IModerationAgent
{
    private readonly IChatCompletionService _chat;
    private readonly ILogger<ModerationAgent> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private const string SystemPrompt = """
        You are FreeGency's content moderation agent for chat messages and reviews.
        Understand Egyptian Arabic, Modern Standard Arabic, English, Arabizi/Franco-Arabic,
        mixed languages, sarcasm, euphemisms, and implied intent. Judge meaning — never a fixed word list.

        Platform policy (enforce strictly):
        1) Abuse / harassment: insults, humiliation, sexual harassment, threats, or demeaning language
           in any language or spelling style, direct or veiled. Hide it.
        2) PII / contact bypass: phone numbers, emails, messaging-app handles, or asking someone to
           continue privately off FreeGency. Redact or hide.
        3) Off-platform work/pay: any attempt to move the job, payment, or working relationship
           outside FreeGency. This harms the marketplace — hide.
        4) Spam / scam / unrelated solicitation — hide.
        5) ALLOW normal on-platform work talk: milestones, deliverables, scheduling through FreeGency,
           polite disagreement, constructive review criticism.

        Bias: if intent is ambiguous but leans toward abuse, harassment, or leaving the platform → hide.
        Only allow when the content is clearly clean coordination or a fair review.

        Return ONLY one JSON object (no markdown, no extra text):
        {
          "categories": ["clean"|"abuse"|"harassment"|"pii_contact"|"off_platform"|"spam"],
          "confidence": 0.0-1.0,
          "action": "allow"|"redact"|"hide"|"block_submit",
          "userMessage": "short bilingual-friendly warning if action != allow, else \"\"",
          "adminSummary": "short reason in English for admins",
          "redactedText": "optional safer version if action is redact; else omit"
        }

        Action guide:
        - allow: clean / normal coordination
        - redact: contact details present; rest of text can stay
        - hide: abuse, off-platform, harassment, spam
        - block_submit: severe threats, explicit sexual harassment, or clear off-platform payment solicitation
        """;

    public ModerationAgent(IChatCompletionService chat, ILogger<ModerationAgent> logger)
    {
        _chat = chat;
        _logger = logger;
    }

    public async Task<ModerationDecision> ModerateAsync(ModerationRequest request, CancellationToken ct = default)
    {
        var content = (request.Content ?? string.Empty).Trim();
        if (content.Length == 0)
        {
            return new ModerationDecision
            {
                Categories = [ModerationCategory.Clean],
                Confidence = 1f,
                Action = ModerationAction.Allow
            };
        }

        // Structural phone/email only — never a slang dictionary. Intent always goes to the LLM.
        var contactHit = ModerationHeuristics.TryDecideContactOnly(content);

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(12));

            var history = new ChatHistory();
            history.AddSystemMessage(SystemPrompt);
            history.AddUserMessage($"""
                Surface: {request.Surface}
                Language note: content may be Arabic, English, Franco-Arabizi, or mixed. Interpret intent carefully.

                Content:
                <<<
                {content[..Math.Min(content.Length, 2500)]}
                >>>

                Decide now. Prefer hide over allow when the user is harassing someone or trying to leave FreeGency.
                """);

            var response = await _chat.GetChatMessageContentsAsync(history, cancellationToken: timeout.Token);
            var raw = response.FirstOrDefault()?.Content ?? "{}";
            var parsed = Parse(raw);
            if (parsed is null)
            {
                _logger.LogWarning("Moderation LLM parse failed. Raw: {Raw}", Truncate(raw, 400));
                return contactHit ?? FlagForReview("LLM parse failed; flagged for admin review.");
            }

            parsed = Enrich(parsed, content);

            // If LLM allowed but we structurally saw a phone/email, still redact.
            if (parsed.Action == ModerationAction.Allow && contactHit is not null)
                return MergeContactOntoAllow(parsed, contactHit, content);

            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Moderation LLM failed; contact-only fallback.");
            return contactHit ?? FlagForReview("LLM unavailable; allowed pending review.");
        }
    }

    private static ModerationDecision FlagForReview(string summary) => new()
    {
        Categories = [ModerationCategory.Clean],
        Confidence = 0.35f,
        Action = ModerationAction.Allow,
        AdminSummary = summary,
        UsedLlm = false
    };

    private static ModerationDecision MergeContactOntoAllow(
        ModerationDecision llm,
        ModerationDecision contact,
        string content) =>
        new()
        {
            Categories = contact.Categories
                .Concat(llm.Categories.Where(c => c != ModerationCategory.Clean))
                .Distinct()
                .ToList(),
            Confidence = Math.Max(llm.Confidence, contact.Confidence),
            Action = ModerationAction.Redact,
            UserMessage = string.IsNullOrWhiteSpace(llm.UserMessage) ? contact.UserMessage : llm.UserMessage,
            AdminSummary = string.IsNullOrWhiteSpace(llm.AdminSummary)
                ? contact.AdminSummary
                : $"{llm.AdminSummary}; {contact.AdminSummary}",
            RedactedText = contact.RedactedText ?? ModerationHeuristics.Redact(content),
            UsedLlm = true
        };

    private static ModerationDecision Enrich(ModerationDecision decision, string content)
    {
        var cats = decision.Categories.Count == 0
            ? [ModerationCategory.Clean]
            : decision.Categories.ToList();

        var needsRedaction =
            decision.Action is ModerationAction.Redact or ModerationAction.Hide or ModerationAction.BlockSubmit
            && string.IsNullOrWhiteSpace(decision.RedactedText);

        return new ModerationDecision
        {
            Categories = cats,
            Confidence = decision.Confidence,
            Action = decision.Action,
            UserMessage = decision.UserMessage,
            AdminSummary = decision.AdminSummary,
            RedactedText = needsRedaction
                ? ModerationHeuristics.Redact(content)
                : decision.RedactedText,
            UsedLlm = true
        };
    }

    private static ModerationDecision? Parse(string raw)
    {
        var cleaned = StripFences(raw);
        var json = ExtractJsonObject(cleaned) ?? cleaned;
        try
        {
            var dto = JsonSerializer.Deserialize<LlmDecisionDto>(json, JsonOpts);
            if (dto is null) return null;

            var categories = (dto.Categories ?? [])
                .Select(ParseCategory)
                .Distinct()
                .ToList();
            if (categories.Count == 0)
                categories.Add(ModerationCategory.Clean);

            return new ModerationDecision
            {
                Categories = categories,
                Confidence = Math.Clamp(dto.Confidence, 0f, 1f),
                Action = ParseAction(dto.Action),
                UserMessage = dto.UserMessage?.Trim() ?? string.Empty,
                AdminSummary = dto.AdminSummary?.Trim() ?? string.Empty,
                RedactedText = string.IsNullOrWhiteSpace(dto.RedactedText) ? null : dto.RedactedText.Trim(),
                UsedLlm = true
            };
        }
        catch
        {
            return null;
        }
    }

    private static ModerationCategory ParseCategory(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "abuse" => ModerationCategory.Abuse,
            "harassment" => ModerationCategory.Harassment,
            "pii_contact" or "piicontact" or "pii" => ModerationCategory.PiiContact,
            "off_platform" or "offplatform" => ModerationCategory.OffPlatform,
            "spam" => ModerationCategory.Spam,
            _ => ModerationCategory.Clean
        };

    private static ModerationAction ParseAction(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "redact" => ModerationAction.Redact,
            "hide" => ModerationAction.Hide,
            "block_submit" or "blocksubmit" => ModerationAction.BlockSubmit,
            _ => ModerationAction.Allow
        };

    private static string StripFences(string raw)
    {
        var t = raw.Trim();
        if (!t.StartsWith("```", StringComparison.Ordinal)) return t;
        t = Regex.Replace(t, @"^```(?:json)?\s*", "", RegexOptions.IgnoreCase);
        t = Regex.Replace(t, @"\s*```$", "");
        return t.Trim();
    }

    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        return text[start..(end + 1)];
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";

    private sealed class LlmDecisionDto
    {
        public List<string>? Categories { get; set; }
        public float Confidence { get; set; }
        public string? Action { get; set; }
        public string? UserMessage { get; set; }
        public string? AdminSummary { get; set; }
        public string? RedactedText { get; set; }
    }
}
