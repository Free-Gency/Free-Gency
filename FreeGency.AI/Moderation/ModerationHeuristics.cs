using System.Text.RegularExpressions;
using FreeGency.Domain.Enums;

namespace FreeGency.AI.Moderation;

/// <summary>
/// Structural contact redaction only (phones/emails). Intent (abuse, off-platform, slang)
/// is decided by the LLM agent — not by word lists.
/// </summary>
public static partial class ModerationHeuristics
{
    [GeneratedRegex(@"(\+?20|0)?1[0125]\d{8}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EgyptianPhoneRegex();

    [GeneratedRegex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?<!\d)(\+?\d[\d\s\-()]{8,}\d)(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex GenericPhoneRegex();

    /// <summary>
    /// Returns a decision only for clear phone/email patterns. Everything else → null (LLM).
    /// </summary>
    public static ModerationDecision? TryDecideContactOnly(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new ModerationDecision
            {
                Categories = [ModerationCategory.Clean],
                Confidence = 1f,
                Action = ModerationAction.Allow,
                AdminSummary = "Empty content."
            };

        var text = content.Trim();
        var hasPhone = EgyptianPhoneRegex().IsMatch(text) || GenericPhoneRegex().IsMatch(text);
        var hasEmail = EmailRegex().IsMatch(text);
        if (!hasPhone && !hasEmail)
            return null;

        return new ModerationDecision
        {
            Categories = [ModerationCategory.PiiContact],
            Confidence = 0.95f,
            Action = ModerationAction.Redact,
            UserMessage =
                "Personal contact details are not allowed in chat/reviews. Sensitive parts were removed and a warning was issued.",
            AdminSummary = "Structural phone/email pattern detected.",
            RedactedText = Redact(text),
            UsedLlm = false
        };
    }

    public static string Redact(string text)
    {
        var result = EgyptianPhoneRegex().Replace(text, "[hidden phone]");
        result = EmailRegex().Replace(result, "[hidden email]");
        result = GenericPhoneRegex().Replace(result, "[hidden phone]");
        return result;
    }
}
