using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FreeGency.AI.Common;

/// <summary>
/// Shared, robust parsing utilities for AI chat-completion outputs. All agents in
/// this assembly (interviewer, milestone planner, ...) use the same tolerant JSON
/// handling so a slightly malformed model response still degrades gracefully.
/// </summary>
internal static class AiOutputParsing
{
    internal static string SanitizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return message;

        var text = message
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("**", string.Empty, StringComparison.Ordinal)
            .Replace("__", string.Empty, StringComparison.Ordinal)
            .Replace("`", string.Empty, StringComparison.Ordinal);

        text = Regex.Replace(text, @"\n{3,}", "\n\n").Trim();
        return text;
    }

    internal static string StripJsonFences(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        var trimmed = raw.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var withoutFirst = firstNewline >= 0 ? trimmed[(firstNewline + 1)..] : trimmed;
            withoutFirst = withoutFirst.Trim();
            if (withoutFirst.EndsWith("```", StringComparison.Ordinal))
                withoutFirst = withoutFirst[..^3].Trim();
            return withoutFirst;
        }

        return trimmed;
    }

    internal static string RepairJsonStrings(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

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

    internal static string? ExtractMessageField(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
        }

        var match = Regex.Match(json, @"""message""\s*:\s*""((?:\\.|[^""])*)""", RegexOptions.Singleline);
        if (match.Success)
            return match.Groups[1].Value;

        return null;
    }

    internal static string UnescapeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        try
        {
            return JsonSerializer.Deserialize<string>($"\"{value}\"") ?? value;
        }
        catch (JsonException)
        {
            return value.Replace("\\n", "\n");
        }
    }

    internal static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        var cut = value[..maxLength];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > maxLength / 2)
            return $"{cut[..lastSpace].TrimEnd(' ', ',', '.')}…";

        return cut.TrimEnd();
    }
}
