using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.ProposalAssistant;

public sealed class ProposalAssistantChatService
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public ProposalAssistantChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<ProposalAssistantAiResult> AskAsync(
        string contextBlock,
        string userMessage,
        string? command,
        string? focusedApplicantName,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default)
    {
        var historyBlock = history.Count == 0
            ? "(none)"
            : string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));

        var resolvedCommand = string.IsNullOrWhiteSpace(command) ? null : command.Trim().ToLowerInvariant();
        var intentHint = resolvedCommand is null
            ? "Infer intent from the user message. Prefer a structured intent when the user asks to summarize, compare, rank, pick a winner, spot risks, draft, or ask screening questions."
            : $"ACTIVE COMMAND: /{resolvedCommand}. Set intent=\"{resolvedCommand}\" and follow that playbook exactly.";

        var focusHint = string.IsNullOrWhiteSpace(focusedApplicantName)
            ? ""
            : $"Focused applicant name (must match context exactly if present): {focusedApplicantName}";

        var playbook = CommandPlaybook(resolvedCommand);

        var userPayload = $"""
            PROJECT & PROPOSALS CONTEXT:
            {contextBlock}

            RECENT HISTORY:
            {historyBlock}

            {intentHint}
            {focusHint}

            COMMAND TASK:
            {playbook}

            CLIENT MESSAGE:
            {userMessage}
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.ProposalAssistant);
        chatHistory.AddUserMessage(userPayload);

        var response = await _chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? "{}";
        return ParseModelOutput(raw, command);
    }

    internal static ProposalAssistantAiResult ParseModelOutput(string raw, string? command)
    {
        var cleaned = StripJsonFences(raw);
        var jsonCandidate = ExtractJsonObject(cleaned) ?? cleaned;

        // Attempt 1: direct deserialize
        if (TryDeserialize(jsonCandidate, out var parsed) && !string.IsNullOrWhiteSpace(parsed!.Reply))
            return Normalize(parsed, command);

        // Attempt 2: repair common LLM JSON (literal newlines inside strings)
        var repaired = RepairJsonStrings(jsonCandidate);
        if (TryDeserialize(repaired, out parsed) && !string.IsNullOrWhiteSpace(parsed!.Reply))
            return Normalize(parsed, command);

        // Attempt 3: pull fields with JsonDocument after repair
        if (TryReadWithDocument(repaired, out parsed) && !string.IsNullOrWhiteSpace(parsed!.Reply))
            return Normalize(parsed, command);

        // Attempt 4: regex-extract "reply" value only
        var replyOnly = ExtractReplyField(cleaned) ?? ExtractReplyField(raw);
        if (!string.IsNullOrWhiteSpace(replyOnly))
        {
            return Normalize(new ProposalAssistantAiResult
            {
                Reply = UnescapeJsonString(replyOnly),
                Intent = ExtractSimpleField(cleaned, "intent") ?? command ?? "ask",
                Cards = [],
                Chips = [],
                Actions = []
            }, command);
        }

        // Last resort: plain text (never dump raw JSON braces to the user)
        var plain = cleaned.Trim();
        if (plain.StartsWith('{') && plain.Contains("\"reply\"", StringComparison.OrdinalIgnoreCase))
            plain = "I had trouble formatting that answer. Please try again, or use /summarize.";

        return new ProposalAssistantAiResult
        {
            Reply = string.IsNullOrWhiteSpace(plain)
                ? "I couldn’t format a reply. Try /summarize or ask again."
                : Truncate(plain, 1200),
            Intent = string.IsNullOrWhiteSpace(command) ? "ask" : command,
            Cards = [],
            Chips = [],
            Actions = []
        };
    }

    private static ProposalAssistantAiResult Normalize(ProposalAssistantAiResult parsed, string? command)
    {
        parsed.Cards ??= [];
        parsed.Chips ??= [];
        parsed.Actions ??= [];
        // Never surface chips unless clarify — frontend owns essentials
        if (!string.Equals(parsed.Intent, "clarify", StringComparison.OrdinalIgnoreCase))
            parsed.Chips = [];

        if (string.IsNullOrWhiteSpace(parsed.Intent))
            parsed.Intent = string.IsNullOrWhiteSpace(command) ? "ask" : command;
        else if (!string.IsNullOrWhiteSpace(command) &&
                 !string.Equals(parsed.Intent, command, StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(parsed.Intent, "clarify", StringComparison.OrdinalIgnoreCase))
            // Honor the explicit slash command unless the model needs a clarify step
            parsed.Intent = command;

        parsed.Reply = SanitizeReply(UnescapeJsonString(parsed.Reply).Trim());
        return parsed;
    }

    /// <summary>
    /// Strip markdown tables / GUIDs so the chat UI never shows raw pipe tables.
    /// </summary>
    internal static string SanitizeReply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) return reply;

        var lines = reply.Replace("\r\n", "\n").Split('\n');
        var kept = new List<string>();
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0)
            {
                if (kept.Count > 0 && kept[^1].Length > 0) kept.Add("");
                continue;
            }

            var pipeCount = line.Count(c => c == '|');
            if (pipeCount >= 2) continue;
            if (Regex.IsMatch(line, @"^[\s|:\-]{3,}$")) continue;
            if (Regex.IsMatch(line, @"^[-*]{3,}$")) continue;

            line = Regex.Replace(
                line,
                @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
                "");
            line = Regex.Replace(line, @"\bProposal\s*IDs?\b\s*:?\s*", "", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\s{2,}", " ").Trim();
            if (line.Length > 0) kept.Add(line);
        }

        var text = string.Join("\n", kept).Trim();
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        // If still looks like a broken table dump, collapse to first clean sentence-ish line
        if (text.Contains('|') || text.Contains("---"))
        {
            text = Regex.Replace(text, @"\|", " ");
            text = Regex.Replace(text, @"-{3,}", " ");
            text = Regex.Replace(text, @"\s{2,}", " ").Trim();
        }

        return text;
    }

    private static bool TryDeserialize(string json, out ProposalAssistantAiResult? result)
    {
        result = null;
        try
        {
            result = JsonSerializer.Deserialize<ProposalAssistantAiResult>(json, JsonOpts);
            return result is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadWithDocument(string json, out ProposalAssistantAiResult? result)
    {
        result = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;

            var reply = root.TryGetProperty("reply", out var r) ? r.GetString() : null;
            if (string.IsNullOrWhiteSpace(reply)) return false;

            result = new ProposalAssistantAiResult
            {
                Reply = reply!,
                Intent = root.TryGetProperty("intent", out var i) ? i.GetString() ?? "ask" : "ask",
                Cards = [],
                Chips = [],
                Actions = []
            };

            if (root.TryGetProperty("chips", out var chips) && chips.ValueKind == JsonValueKind.Array)
            {
                result.Chips = chips.EnumerateArray()
                    .Select(c => c.GetString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Cast<string>()
                    .ToList();
            }

            if (root.TryGetProperty("cards", out var cards) && cards.ValueKind == JsonValueKind.Array)
            {
                result.Cards = JsonSerializer.Deserialize<List<ProposalAssistantAiCard>>(cards.GetRawText(), JsonOpts) ?? [];
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        if (start < 0) return null;

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
            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                    return text[start..(i + 1)];
            }
        }

        return null;
    }

    /// <summary>
    /// Replace literal newlines/tabs inside JSON string values with escaped sequences
    /// so System.Text.Json can parse LLM output that breaks the JSON spec.
    /// </summary>
    private static string RepairJsonStrings(string json)
    {
        var sb = new StringBuilder(json.Length + 32);
        var inString = false;
        var escape = false;

        foreach (var c in json)
        {
            if (inString)
            {
                if (escape)
                {
                    sb.Append(c);
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    sb.Append(c);
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    sb.Append(c);
                    inString = false;
                    continue;
                }

                if (c == '\n') { sb.Append("\\n"); continue; }
                if (c == '\r') { sb.Append("\\r"); continue; }
                if (c == '\t') { sb.Append("\\t"); continue; }

                sb.Append(c);
                continue;
            }

            if (c == '"')
            {
                inString = true;
                sb.Append(c);
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static string? ExtractReplyField(string text)
    {
        // "reply"\s*:\s*" ... "  with support for escaped quotes
        var match = Regex.Match(
            text,
            "\"reply\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (match.Success)
            return match.Groups[1].Value;

        // Multiline broken reply: "reply": ".... until next "intent"
        var loose = Regex.Match(
            text,
            "\"reply\"\\s*:\\s*\"(?<body>[\\s\\S]*?)\"\\s*,\\s*\"intent\"",
            RegexOptions.IgnoreCase);

        return loose.Success ? loose.Groups["body"].Value : null;
    }

    private static string? ExtractSimpleField(string text, string field)
    {
        var match = Regex.Match(
            text,
            $"\"{Regex.Escape(field)}\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string UnescapeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value
            .Replace("\\n", "\n", StringComparison.Ordinal)
            .Replace("\\r", "\r", StringComparison.Ordinal)
            .Replace("\\t", "\t", StringComparison.Ordinal)
            .Replace("\\\"", "\"", StringComparison.Ordinal)
            .Replace("\\\\", "\\", StringComparison.Ordinal);
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

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max] + "…";

    private static string CommandPlaybook(string? command) => command switch
    {
        "summarize" =>
            "Produce a per-proposal essentials pass. Card for every applicant. Each insight must cite a concrete signal (skill overlap, budget, letter quality).",
        "compare" =>
            "Head-to-head of exactly two applicants. Prefer focused names if provided (e.g. 'A vs B'); else the two strongest by rubric. Contrasting insights required.",
        "bestfit" =>
            "Pick ONE winner. Name them in reply. Explain the deciding axis and one caveat. Exactly one card.",
        "rank" =>
            "Ordered shortlist best→worst (max 5 cards). Position-specific insights. Reply states the ranking thesis.",
        "redflags" =>
            "Risk scan only. Cards solely for applicants with concrete flags. If none, say so and return empty cards.",
        "profile" =>
            "Deep dive on the focused applicant. One card. Hiring recommendation in insight (interview / maybe / skip).",
        "draft" =>
            "Write only the client outbound message in reply. No cards. Reference project title + one proposal-specific detail.",
        "questions" =>
            "Write 4-6 numbered screening questions for the focused applicant (or clarify who). Probe gaps vs RequiredSkills and their cover letter.",
        "why" =>
            "Explain the current best fit in 3 bullets (skills, budget, letter/reputation). Optional one card.",
        "help" =>
            "List commands briefly under Analyze / Decide / Act. No cards.",
        _ =>
            "Answer the client’s question using the rubric. Use cards only when naming specific applicants."
    };
}

public sealed class ProposalAssistantAiResult
{
    [JsonPropertyName("reply")]
    public string Reply { get; set; } = "";

    [JsonPropertyName("intent")]
    public string Intent { get; set; } = "ask";

    [JsonPropertyName("cards")]
    public List<ProposalAssistantAiCard> Cards { get; set; } = [];

    [JsonPropertyName("chips")]
    public List<string> Chips { get; set; } = [];

    [JsonPropertyName("actions")]
    public List<ProposalAssistantAiAction> Actions { get; set; } = [];
}

public sealed class ProposalAssistantAiCard
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "profile";

    [JsonPropertyName("applicantName")]
    public string ApplicantName { get; set; } = "";

    [JsonPropertyName("proposalId")]
    public string? ProposalId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("teamId")]
    public string? TeamId { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("reviewCount")]
    public int? ReviewCount { get; set; }

    [JsonPropertyName("skills")]
    public List<string>? Skills { get; set; }

    [JsonPropertyName("highlights")]
    public List<string>? Highlights { get; set; }

    [JsonPropertyName("proposedBudget")]
    public decimal? ProposedBudget { get; set; }

    [JsonPropertyName("insight")]
    public string? Insight { get; set; }
}

public sealed class ProposalAssistantAiAction
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("teamId")]
    public string? TeamId { get; set; }

    [JsonPropertyName("proposalId")]
    public string? ProposalId { get; set; }

    [JsonPropertyName("projectId")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
