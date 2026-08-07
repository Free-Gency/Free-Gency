using System.Text;
using System.Text.RegularExpressions;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Normalizes message text so profanity and abuse can be matched reliably
/// across Arabic letter variants, diacritics, zero-width characters, and spacing.
/// Thread-safe and stateless.
/// </summary>
public sealed partial class MessageNormalizer
{
    private static readonly Regex WhitespaceRegex = WhitespaceRegexFactory();

    public string Normalize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        var text = message.Normalize(NormalizationForm.FormKC);
        var builder = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            if (IsZeroWidth(ch))
                continue;

            if (IsArabicDiacritic(ch))
                continue;

            if (ch == '\u0640')
                continue;

            builder.Append(MapArabic(ch));
        }

        var result = WhitespaceRegex.Replace(builder.ToString(), " ").Trim();

        // Normalize emoji variation selectors so plain and variation-selector
        // forms of the same emoji match each other.
        result = result.Replace("\uFE0F", string.Empty);

        return result;
    }

    private static char MapArabic(char ch)
    {
        return ch switch
        {
            'أ' or 'إ' or 'آ' or 'ٱ' => 'ا',
            'ى' => 'ي',
            'ة' => 'ه',
            _ => ch
        };
    }

    private static bool IsZeroWidth(char ch)
    {
        return ch is '\u200B' or '\u200C' or '\u200D' or '\u200E' or '\u200F' or '\u2060' or '\uFEFF';
    }

    private static bool IsArabicDiacritic(char ch)
    {
        return (ch >= '\u064B' && ch <= '\u065F')
            || ch is '\u0670' or '\u06D6' or '\u06D7' or '\u06D8' or '\u06D9' or '\u06DA'
                or '\u06DB' or '\u06DC' or '\u06DF' or '\u06E0' or '\u06E1' or '\u06E2'
                or '\u06E3' or '\u06E4' or '\u06E7' or '\u06E8' or '\u06EA' or '\u06EB'
                or '\u06EC' or '\u06ED';
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegexFactory();
}
