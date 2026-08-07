using System.Globalization;
using System.Text.Json;

namespace FreeGency.AI.ReviewModeration.Providers;

/// <summary>
/// Defensive JSON parser for the review moderation model output. Never throws:
/// unparseable input returns <c>false</c> so the reliability layer can retry or
/// fall back to manual review.
/// </summary>
public sealed class ReviewModerationJsonParser
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Attempts to parse the raw model output into a <see cref="ReviewAiResponse"/>.
    /// Handles markdown fences, missing fields, invalid values, and nulls.
    /// </summary>
    public bool TryParse(string? raw, out ReviewAiResponse? result)
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

            result = new ReviewAiResponse
            {
                RiskScore = GetDouble(root, "riskScore"),
                Confidence = GetDouble(root, "confidence"),
                RiskLevel = GetString(root, "riskLevel"),
                Action = GetString(root, "action"),
                Sentiment = GetString(root, "sentiment"),
                SentimentConfidence = GetDouble(root, "sentimentConfidence"),
                QualityScore = GetDouble(root, "qualityScore"),
                QualityBreakdown = GetQualityBreakdown(root, "qualityBreakdown"),
                Constructive = GetBool(root, "constructive"),
                ConstructivenessScore = GetDouble(root, "constructivenessScore"),
                AuthenticityScore = GetDouble(root, "authenticityScore"),
                RatingConsistency = GetString(root, "ratingConsistency"),
                LengthCategory = GetString(root, "lengthCategory"),
                WritingStyle = GetString(root, "writingStyle"),
                Language = GetString(root, "language"),
                Toxicity = GetDouble(root, "toxicity"),
                Summary = GetSummary(root, "summary"),
                FlatSummary = GetFlatSummary(root, "summary"),
                Strengths = GetStringList(root, "strengths"),
                Weaknesses = GetStringList(root, "weaknesses"),
                Keywords = GetStringList(root, "keywords"),
                SecurityCategories = GetStringList(root, "securityCategories"),
                Reason = GetString(root, "reason"),
                Categories = GetCategories(root, "categories"),
                Entities = GetEntities(root, "entities"),
                SuggestedTags = GetStringList(root, "suggestedTags"),
                Suggestions = GetSuggestions(root, "suggestions"),
                Recommendation = GetString(root, "recommendation"),
                MaskedReview = GetString(root, "maskedReview")
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

    private static double? GetDouble(JsonElement obj, string name)
    {
        if (TryGetProperty(obj, name, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
                return number;

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

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

            if (value.ValueKind == JsonValueKind.String &&
                bool.TryParse(value.GetString(), out var parsed))
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

    private static List<ReviewRawCategory> GetCategories(JsonElement obj, string name)
    {
        var result = new List<ReviewRawCategory>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    var category = GetString(item, "category");
                    if (!string.IsNullOrWhiteSpace(category))
                        result.Add(new ReviewRawCategory(category, GetDouble(item, "score")));
                }
                else if (item.ValueKind == JsonValueKind.String)
                {
                    var category = item.GetString();
                    if (!string.IsNullOrWhiteSpace(category))
                        result.Add(new ReviewRawCategory(category, 0.5));
                }
            }
        }

        return result;
    }

    private static List<ReviewRawEntity> GetEntities(JsonElement obj, string name)
    {
        var result = new List<ReviewRawEntity>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                var type = GetString(item, "type");
                var entityValue = GetString(item, "value");

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(entityValue))
                    continue;

                result.Add(new ReviewRawEntity(type, entityValue, GetDouble(item, "confidence")));
            }
        }

        return result;
    }

    private static List<ReviewRawSuggestion> GetSuggestions(JsonElement obj, string name)
    {
        var result = new List<ReviewRawSuggestion>();

        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                var type = GetString(item, "type");
                var message = GetString(item, "message");

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(message))
                    continue;

                result.Add(new ReviewRawSuggestion(type, message, GetString(item, "severity") ?? "Medium"));
            }
        }

        return result;
    }

    private static ReviewRawSummary? GetSummary(JsonElement obj, string name)
    {
        if (!TryGetProperty(obj, name, out var value) || value.ValueKind != JsonValueKind.Object)
            return null;

        return new ReviewRawSummary(
            GetString(value, "shortSummary"),
            GetStringList(value, "positivePoints"),
            GetStringList(value, "negativePoints"),
            GetStringList(value, "keyTopics"));
    }

    private static ReviewRawQualityBreakdown? GetQualityBreakdown(JsonElement obj, string name)
    {
        if (!TryGetProperty(obj, name, out var value) || value.ValueKind != JsonValueKind.Object)
            return null;

        return new ReviewRawQualityBreakdown(
            GetDouble(value, "grammar"),
            GetDouble(value, "spelling"),
            GetDouble(value, "readability"),
            GetDouble(value, "professionalTone"),
            GetDouble(value, "constructiveness"),
            GetDouble(value, "helpfulness"),
            GetDouble(value, "clarity"),
            GetDouble(value, "specificDetails"),
            GetDouble(value, "length"),
            GetDouble(value, "relevance"),
            GetDouble(value, "originality"));
    }

    private static string? GetFlatSummary(JsonElement obj, string name)
    {
        if (TryGetProperty(obj, name, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString();

        return null;
    }
}
