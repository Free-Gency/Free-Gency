using System.Text;
using System.Text.RegularExpressions;

namespace FreeGency.AI.Guardrails;

/// <summary>
/// Shared, deterministic text helpers used by every guardrail detector. Provides
/// a consistent normalization pipeline (leet/Arabizi digits to letters, masking
/// removal, repeated-character collapsing, whitespace folding) so creative,
/// masked, and leet spellings resolve to the same canonical form as the curated
/// word lists. Stateless and thread-safe.
/// </summary>
public static class GuardrailText
{
    private static readonly Regex WordRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Normalizes a token: digits become letters (English leet and Arabizi
    /// conventions), everything that is not a letter is dropped (including
    /// masking asterisks), letters are lowercased, and consecutive repeats are
    /// collapsed. Returns the canonical form used for list matching.
    /// </summary>
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length);
        char? previous = null;

        foreach (var ch in value)
        {
            var c = ToLetter(ch);
            if (c == '\0')
                continue;

            if (c == previous)
                continue;

            sb.Append(c);
            previous = c;
        }

        return sb.ToString();
    }

    /// <summary>Returns true when the character belongs to the Arabic script block.</summary>
    public static bool IsArabicScript(char ch)
        => ch is >= '\u0600' and <= '\u06FF' or >= '\u0750' and <= '\u077F' or >= '\uFB50' and <= '\uFDFF' or >= '\uFE70' and <= '\uFEFF';

    /// <summary>Returns true when the character belongs to the basic Latin script.</summary>
    public static bool IsLatinScript(char ch)
        => ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    /// <summary>
    /// Removes spaces and punctuation so phrases can be searched on the compact
    /// normalized form (for example "ignore all previous instructions").
    /// </summary>
    public static string Compact(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            var c = ToLetter(ch);
            if (c == '\0' || c == ' ')
                continue;

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>Extracts the alphanumeric tokens from <paramref name="value"/>.</summary>
    public static List<string> Tokenize(string value)
    {
        var result = new List<string>();
        foreach (Match match in WordRegex.Matches(value))
            result.Add(match.Value.ToLowerInvariant());

        return result;
    }

    /// <summary>Counts emoji code points in <paramref name="value"/>.</summary>
    public static int CountEmojis(string value)
    {
        var count = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                var codePoint = char.ConvertToUtf32(value[i], value[i + 1]);
                if (IsEmojiCodePoint(codePoint))
                    count++;
                i++;
            }
            else if (IsEmojiCodePoint(value[i]))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Counts the distinct non-whitespace characters in <paramref name="value"/>.</summary>
    public static int CountDistinctNonWhitespace(string value)
    {
        var seen = new HashSet<char>();
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
                continue;

            seen.Add(char.ToLowerInvariant(ch));
        }

        return seen.Count;
    }

    private static char ToLetter(char ch)
    {
        if (ch is >= '0' and <= '9')
        {
            return ch switch
            {
                '0' => 'o',
                '1' => 'i',
                '2' => 'z',
                '3' => 'e',
                '4' => 'a',
                '5' => 's',
                '6' => 'g',
                '7' => 't',
                '8' => 'b',
                '9' => 'g',
                _ => ch
            };
        }

        if (ch == ' ' || char.IsLetter(ch))
            return char.ToLowerInvariant(ch);

        return '\0';
    }

    private static bool IsEmojiCodePoint(int codePoint)
        => (codePoint is >= 0x1F000 and <= 0x1FAFF)
        || (codePoint is >= 0x1F1E6 and <= 0x1F1FF)
        || (codePoint is >= 0x2600 and <= 0x27BF)
        || (codePoint is >= 0x2B00 and <= 0x2BFF)
        || codePoint == 0xFE0F;
}
