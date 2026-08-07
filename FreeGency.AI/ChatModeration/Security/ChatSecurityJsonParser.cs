using System.Text.Json;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Defensive JSON parser for the moderation model output. Never throws:
/// unparseable input returns <c>false</c> so callers can retry or fall back.
/// </summary>
public sealed class ChatSecurityJsonParser
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Attempts to parse the raw model output into a <see cref="ChatSecurityAiResponse"/>.
    /// Handles markdown fences, missing fields, invalid enums, and nulls.
    /// </summary>
    public bool TryParse(string? raw, out ChatSecurityAiResponse? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var text = StripCodeFences(raw);
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');

        if (start < 0 || end <= start)
            return false;

        var json = text.Substring(start, end - start + 1);

        try
        {
            using var document = JsonDocument.Parse(json, DocumentOptions);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return false;

            var hasAnyField = false;
            foreach (var _ in root.EnumerateObject())
            {
                hasAnyField = true;
                break;
            }

            if (!hasAnyField)
                return false;

            result = new ChatSecurityAiResponse
            {
                IsSafe = GetBool(root, "isSafe") ?? false,
                RiskScore = GetDouble(root, "riskScore"),
                Confidence = GetDouble(root, "confidence"),
                RiskLevel = GetString(root, "riskLevel"),
                Action = GetString(root, "action"),
                Reason = GetString(root, "reason"),
                DetectedLanguages = GetStringList(root, "detectedLanguages"),
                Categories = GetCategories(root, "categories"),
                MatchedKeywords = GetStringList(root, "matchedKeywords"),
                DetectedEntities = GetEntities(root, "detectedEntities"),
                MaskedMessage = GetString(root, "maskedMessage")
            };

            return true;
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
            var lines = trimmed.Split('\n');
            if (lines.Length > 2)
                return string.Join('\n', lines[1..^1]).Trim();
        }
        return trimmed;
    }

    private static bool TryGetProperty(JsonElement obj, string name, out JsonElement value)
    {
        if (obj.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in obj.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string? GetString(JsonElement obj, string name)
    {
        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString();

        return null;
    }

    private static bool? GetBool(JsonElement obj, string name)
    {
        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.True)
            return true;

        if (TryGetProperty(obj, name, out value) && value.ValueKind == JsonValueKind.False)
            return false;

        return null;
    }

    private static double? GetDouble(JsonElement obj, string name)
    {
        if (TryGetProperty(obj, name, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
                return number;

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(value.GetString(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        return null;
    }

    private static int GetInt(JsonElement obj, string name)
    {
        if (TryGetProperty(obj, name, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                return number;

            if (value.ValueKind == JsonValueKind.String &&
                int.TryParse(value.GetString(), out var parsed))
                return parsed;
        }

        return 0;
    }

    private static List<string> GetStringList(JsonElement obj, string name)
    {
        var result = new List<string>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                    result.Add(item.GetString() ?? string.Empty);
            }
        }

        return result;
    }

    private static List<ChatSecurityCategoryDto> GetCategories(JsonElement obj, string name)
    {
        var result = new List<ChatSecurityCategoryDto>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    result.Add(new ChatSecurityCategoryDto { Category = item.GetString() });
                }
                else if (item.ValueKind == JsonValueKind.Object)
                {
                    result.Add(new ChatSecurityCategoryDto
                    {
                        Category = GetString(item, "category"),
                        Score = GetDouble(item, "score")
                    });
                }
            }
        }

        return result;
    }

    private static List<ChatSecurityEntityDto> GetEntities(JsonElement obj, string name)
    {
        var result = new List<ChatSecurityEntityDto>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                result.Add(new ChatSecurityEntityDto
                {
                    Type = GetString(item, "type"),
                    Value = GetString(item, "value"),
                    StartIndex = GetInt(item, "startIndex"),
                    EndIndex = GetInt(item, "endIndex"),
                    Confidence = GetDouble(item, "confidence")
                });
            }
        }

        return result;
    }
}
