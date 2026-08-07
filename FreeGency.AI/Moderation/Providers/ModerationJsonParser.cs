using System.Text.Json;
using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation.Providers;

/// <summary>
/// Defensive JSON parser for the moderation model output. Never throws:
/// unparseable input returns <c>false</c> so the reliability layer can retry
/// or fall back to manual review.
/// </summary>
public sealed class ModerationJsonParser
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Attempts to parse the raw model output into a <see cref="ModerationAiResponse"/>.
    /// Handles markdown fences, missing fields, invalid values, and nulls.
    /// </summary>
    public bool TryParse(string? raw, out ModerationAiResponse? result)
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

            result = new ModerationAiResponse
            {
                IsSafe = GetBool(root, "isSafe") ?? false,
                RiskScore = GetDouble(root, "riskScore"),
                Confidence = GetDouble(root, "confidence"),
                RiskLevel = GetString(root, "riskLevel"),
                Action = GetString(root, "action"),
                Reason = GetString(root, "reason"),
                Categories = GetCategories(root, "categories"),
                FlaggedKeywords = GetStringList(root, "flaggedKeywords")
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
        if (TryGetProperty(obj, name, out var value))
        {
            if (value.ValueKind == JsonValueKind.True)
                return true;

            if (value.ValueKind == JsonValueKind.False)
                return false;
        }

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

    private static List<ModerationCategoryScore> GetCategories(JsonElement obj, string name)
    {
        var result = new List<ModerationCategoryScore>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    var category = GetString(item, "category");
                    if (!string.IsNullOrWhiteSpace(category))
                        result.Add(new ModerationCategoryScore(category, Math.Clamp(GetDouble(item, "score") ?? 0.5, 0, 1)));
                }
                else if (item.ValueKind == JsonValueKind.String)
                {
                    var category = item.GetString();
                    if (!string.IsNullOrWhiteSpace(category))
                        result.Add(new ModerationCategoryScore(category, 0.5));
                }
            }
        }

        return result;
    }
}
