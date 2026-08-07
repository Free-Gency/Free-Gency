using System.Text.Json;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Models;

namespace FreeGency.AI.ChatModeration.Helpers;

public sealed class ChatModerationJsonParser : IChatModerationJsonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool TryParse(string? raw, out ModerationAiResult? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var cleaned = StripCodeFences(raw);
        var jsonStart = cleaned.IndexOf('{');
        var jsonEnd = cleaned.LastIndexOf('}');

        if (jsonStart < 0 || jsonEnd <= jsonStart)
            return false;

        var json = cleaned[jsonStart..(jsonEnd + 1)];

        try
        {
            result = JsonSerializer.Deserialize<ModerationAiResult>(json, JsonOptions);
            return result is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = trimmed.IndexOf('\n');
            if (firstNewLine > 0)
                trimmed = trimmed[(firstNewLine + 1)..];
        }

        if (trimmed.EndsWith("```", StringComparison.Ordinal))
            trimmed = trimmed[..^3];

        return trimmed.Trim();
    }
}
