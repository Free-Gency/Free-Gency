using System.Text.RegularExpressions;

namespace FreeGency.AI.Moderation.Observability;

/// <summary>
/// Redacts sensitive values (emails, phone numbers, credit cards, IBANs,
/// passwords, tokens, API keys, JWTs) from strings before they reach logs.
/// The moderation core never logs raw content; this is a defensive last line.
/// </summary>
public static partial class ModerationLogSanitizer
{
    private static readonly Regex EmailRegex = SensitiveEmailRegex();
    private static readonly Regex PhoneRegex = SensitivePhoneRegex();
    private static readonly Regex CardRegex = SensitiveCardRegex();
    private static readonly Regex IbanRegex = SensitiveIbanRegex();
    private static readonly Regex SecretRegex = SensitiveSecretRegex();
    private static readonly Regex JwtRegex = SensitiveJwtRegex();

    /// <summary>
    /// Returns <paramref name="value"/> with sensitive substrings replaced by
    /// <c>[REDACTED]</c>. Null input returns <see cref="string.Empty"/>.
    /// </summary>
    public static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var result = value;
        result = EmailRegex.Replace(result, "[REDACTED]");
        result = PhoneRegex.Replace(result, "[REDACTED]");
        result = CardRegex.Replace(result, "[REDACTED]");
        result = IbanRegex.Replace(result, "[REDACTED]");
        result = JwtRegex.Replace(result, "[REDACTED]");
        result = SecretRegex.Replace(result, "[REDACTED]");
        return result;
    }

    [GeneratedRegex(@"\b[\w.+-]+@[\w-]+(?:\.[\w-]+)+\b", RegexOptions.CultureInvariant)]
    private static partial Regex SensitiveEmailRegex();

    [GeneratedRegex(@"(?<!\d)\+?\d[\d\s().-]{7,}\d(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex SensitivePhoneRegex();

    [GeneratedRegex(@"\b(?:\d[ -]?){13,19}\b", RegexOptions.CultureInvariant)]
    private static partial Regex SensitiveCardRegex();

    [GeneratedRegex(@"\b[A-Z]{2}\d{2}[A-Z0-9]{11,30}\b", RegexOptions.CultureInvariant)]
    private static partial Regex SensitiveIbanRegex();

    [GeneratedRegex(@"\beyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\b", RegexOptions.CultureInvariant)]
    private static partial Regex SensitiveJwtRegex();

    [GeneratedRegex(
        @"\b(password|passwd|pwd|secret|token|api[\s_-]?key|apikey|private[\s_-]?key)\b\s*[:=]\s*[^\s,;]+",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveSecretRegex();
}
