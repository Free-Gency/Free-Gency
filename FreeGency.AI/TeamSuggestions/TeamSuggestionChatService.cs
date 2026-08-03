using System.Text.Json;
using FreeGency.AI.DTOs;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.TeamSuggestions;

public sealed class TeamSuggestionChatService
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public TeamSuggestionChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<TeamSuggestionAiResult> SuggestAsync(TeamSuggestionRequest request, CancellationToken ct = default)
    {
        var candidateBlock = string.Join("\n", request.Candidates.Select(c =>
            $"- Team: {c.Name} (id: {c.TeamId})\n" +
            $"  Skills: {string.Join(", ", c.Skills)}\n" +
            $"  Specialties: {string.Join(", ", c.Specialties)}\n" +
            $"  Categories: {string.Join(", ", c.Categories)}\n" +
            $"  Members: {c.MemberCount} | Rating: {c.AverageRating:N1} ({c.RatingCount} reviews)\n" +
            $"  OPEN JOBS:\n" + string.Join("\n", c.OpenJobs.Select(j =>
                $"    - Job: {j.Title} (id: {j.JobId}) | skills: {string.Join(", ", j.Skills)} | {j.Description}"))));

        var userPayload = $"""
            DEVELOPER PROFILE:
            Name: {request.DeveloperName}
            Skills: {string.Join(", ", request.DeveloperSkills)}
            Specialties: {string.Join(", ", request.DeveloperSpecialties)}
            Categories: {string.Join(", ", request.DeveloperCategories)}

            CANDIDATE TEAMS (each with their open jobs):
            {candidateBlock}

            Return ONLY valid JSON per the system instructions. Top K: {request.TopK}.
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.TeamSuggestion);
        chatHistory.AddUserMessage(userPayload);

        var response = await _chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? string.Empty;

        return ParseModelOutput(raw);
    }

    internal static TeamSuggestionAiResult ParseModelOutput(string raw)
    {
        var cleaned = StripJsonFences(raw);
        var jsonCandidate = ExtractJsonObject(cleaned) ?? cleaned;

        if (TryDeserialize(jsonCandidate, out var parsed) && parsed!.Candidates.Count > 0)
            return parsed!;

        var repaired = RepairJsonStrings(jsonCandidate);
        if (TryDeserialize(repaired, out parsed) && parsed!.Candidates.Count > 0)
            return parsed!;

        return new TeamSuggestionAiResult
        {
            Candidates = [],
            OverallSummary = "I couldn’t parse a ranking this time. Please try again."
        };
    }

    private static bool TryDeserialize(string json, out TeamSuggestionAiResult? result)
    {
        result = null;
        try
        {
            result = JsonSerializer.Deserialize<TeamSuggestionAiResult>(json, JsonOpts);
            return result is not null;
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

    private static string RepairJsonStrings(string json)
    {
        var sb = new System.Text.StringBuilder(json.Length + 32);
        var inString = false;
        var escape = false;

        foreach (var c in json)
        {
            if (inString)
            {
                if (escape) { sb.Append(c); escape = false; continue; }
                if (c == '\\') { sb.Append(c); escape = true; continue; }
                if (c == '"') { sb.Append(c); inString = false; continue; }
                if (c == '\n') { sb.Append("\\n"); continue; }
                if (c == '\r') { sb.Append("\\r"); continue; }
                if (c == '\t') { sb.Append("\\t"); continue; }
                sb.Append(c);
                continue;
            }

            if (c == '"') { inString = true; sb.Append(c); continue; }
            sb.Append(c);
        }

        return sb.ToString();
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
}