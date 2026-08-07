using System.Text.RegularExpressions;
using FreeGency.AI.ChatModeration.Constants;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Detects the dominant language of a message: Arabic, English, Franco Arabic,
/// mixed, emoji, or programming code. Thread-safe and stateless.
/// </summary>
public sealed partial class ContentLanguageDetector
{
    public ContentLanguage Detect(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return ContentLanguage.Unknown;

        var arabic = 0;
        var latin = 0;
        var emoji = 0;

        foreach (var ch in message)
        {
            if (IsArabic(ch))
                arabic++;
            else if (IsLatin(ch))
                latin++;
            else if (IsEmojiCodeUnit(ch))
                emoji++;
        }

        if (emoji > 0 && arabic + latin == 0)
            return ContentLanguage.Emoji;

        if (latin > 0 && LooksLikeCode(message))
            return ContentLanguage.Programming;

        if (arabic > 0 && latin > 0)
            return ContentLanguage.Mixed;

        if (arabic > 0)
            return ContentLanguage.Arabic;

        if (latin > 0)
            return LooksLikeFranco(message) ? ContentLanguage.Franco : ContentLanguage.English;

        return ContentLanguage.Unknown;
    }

    private static bool IsArabic(char ch)
    {
        return (ch >= '\u0600' && ch <= '\u06FF') || (ch >= '\u0750' && ch <= '\u077F');
    }

    private static bool IsLatin(char ch)
    {
        return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
    }

    private static bool IsEmojiCodeUnit(char ch)
    {
        return (ch >= '\u2600' && ch <= '\u27BF')
            || (ch >= '\u2B00' && ch <= '\u2BFF')
            || (ch >= '\uD800' && ch <= '\uDBFF')
            || ch == '\uFE0F';
    }

    private static bool LooksLikeFranco(string message)
    {
        if (FrancoRegex().IsMatch(message))
            return true;

        foreach (var keyword in ModerationLexicon.FrancoProfanity)
        {
            if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool LooksLikeCode(string message)
    {
        if (message.Contains('{') && message.Contains('}'))
            return true;

        if (message.Contains("=>"))
            return true;

        if (message.Contains(';'))
            return true;

        return CodeRegex().IsMatch(message);
    }

    [GeneratedRegex(@"\b(?:[a-zA-Z]+[3579][a-zA-Z]+|[3579][a-zA-Z]{3,}|[a-zA-Z]{3,}[3579])\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FrancoRegex();

    [GeneratedRegex(@"\b(function|return|def|class|public|private|import|const|var|let|SELECT|INSERT|UPDATE|DELETE|console\.log)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();
}
