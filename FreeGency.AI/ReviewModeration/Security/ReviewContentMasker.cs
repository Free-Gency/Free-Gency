using System.Text;
using System.Text.RegularExpressions;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Security;

/// <summary>
/// Deterministic content masker. Detects and masks profanity (Arabic, English,
/// Franco Arabic, masked, and leet spellings) and sensitive values (emails,
/// phones, credit cards, IBANs, crypto wallets, API keys, passwords, external
/// URLs). Stateless and thread-safe; the patterns and word lists are built once
/// and shared. Masking never depends on the AI, so sensitive values are masked
/// even when the AI service is unavailable.
/// </summary>
public sealed class ReviewContentMasker : IReviewContentMasker
{
    private static readonly Regex EmailRegex = new(@"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\+?\d[\d\s().\-]{5,16}\d", RegexOptions.Compiled);
    private static readonly Regex CreditCardRegex = new(@"\d[\d\s\-]{11,17}\d", RegexOptions.Compiled);
    private static readonly Regex IbanRegex = new(@"\b[A-Z]{2}\d{2}[A-Z0-9]{11,30}\b", RegexOptions.Compiled);
    private static readonly Regex BtcWalletRegex = new(@"\b(bc1|[13])[A-HJ-NP-Za-km-z1-9]{25,39}\b", RegexOptions.Compiled);
    private static readonly Regex EthWalletRegex = new(@"\b0x[a-fA-F0-9]{40}\b", RegexOptions.Compiled);
    private static readonly Regex ApiKeyRegex = new(@"\b(?:sk-[A-Za-z0-9]{16,}|pk-[A-Za-z0-9]{16,}|AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{20,}|xox[bap]-[A-Za-z0-9\-]{10,}|AIza[0-9A-Za-z\-]{20,})\b", RegexOptions.Compiled);
    private static readonly Regex PasswordRegex = new(@"\b(?:password|passwd|pwd|secret)\b\s*(?::|=|is\s+)\s*\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HttpUrlRegex = new(@"https?://[^\s<>""]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HandleUrlRegex = new(@"\b(?:wa\.me|t\.me|telegram\.me|discord\.(?:gg|com)|whatsapp\.com|facebook\.com|fb\.com|instagram\.com|linkedin\.com|github\.com|bit\.ly|shorturl\.at)/[^\s<>""]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> ProfanityTokens = BuildProfanityTokens();
    private static readonly string[] ProfanityPhrases = BuildProfanityPhrases();

    /// <inheritdoc />
    public IReadOnlyList<ReviewSensitiveMatch> FindSensitive(string text)
    {
        var result = new List<ReviewSensitiveMatch>();
        if (string.IsNullOrEmpty(text))
            return result;

        if (EmailRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("Email", ReviewSecurityCategory.Advertisement));
        if (PhoneRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("Phone", ReviewSecurityCategory.Advertisement));
        if (HttpUrlRegex.IsMatch(text) || HandleUrlRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("Url", ReviewSecurityCategory.Advertisement));
        if (CreditCardRegex.IsMatch(text) && ContainsValidCard(text))
            result.Add(new ReviewSensitiveMatch("CreditCard", ReviewSecurityCategory.SensitiveInformation));
        if (IbanRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("Iban", ReviewSecurityCategory.SensitiveInformation));
        if (BtcWalletRegex.IsMatch(text) || EthWalletRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("Wallet", ReviewSecurityCategory.SensitiveInformation));
        if (ApiKeyRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("ApiKey", ReviewSecurityCategory.SensitiveInformation));
        if (PasswordRegex.IsMatch(text))
            result.Add(new ReviewSensitiveMatch("Password", ReviewSecurityCategory.SensitiveInformation));

        return result;
    }

    /// <inheritdoc />
    public bool ContainsProfanity(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = Normalize(text);

        foreach (var token in GetTokens(normalized))
        {
            if (token.Normalized.Length >= 2 && ProfanityTokens.Contains(token.Normalized))
                return true;
        }

        var compact = BuildCompact(normalized, out var map);
        if (map.Length == 0 || ProfanityPhrases.Length == 0)
            return false;

        foreach (var phrase in ProfanityPhrases)
        {
            if (compact.Contains(phrase, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public string? Mask(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var ranges = FindAllRanges(text);
        if (ranges.Count == 0)
            return null;

        var chars = text.ToCharArray();
        foreach (var (start, end) in ranges)
        {
            for (var i = start; i <= end; i++)
                chars[i] = '*';
        }

        return new string(chars);
    }

    private static List<(int Start, int End)> FindAllRanges(string text)
    {
        var ranges = new List<(int, int)>();

        if (!string.IsNullOrWhiteSpace(text))
        {
            AddProfanityRanges(text, ranges);
            AddRegexRanges(text, EmailRegex, ranges);
            AddValidatedRanges(text, PhoneRegex, ranges, minDigits: 7, maxDigits: 15, validator: null);
            AddValidatedRanges(text, CreditCardRegex, ranges, minDigits: 13, maxDigits: 19, validator: IsValidLuhn);
            AddRegexRanges(text, IbanRegex, ranges);
            AddRegexRanges(text, BtcWalletRegex, ranges);
            AddRegexRanges(text, EthWalletRegex, ranges);
            AddRegexRanges(text, ApiKeyRegex, ranges);
            AddRegexRanges(text, PasswordRegex, ranges);
            AddRegexRanges(text, HttpUrlRegex, ranges);
            AddRegexRanges(text, HandleUrlRegex, ranges);
        }

        return NormalizeRanges(ranges);
    }

    private static void AddProfanityRanges(string text, List<(int, int)> ranges)
    {
        var normalized = Normalize(text);

        foreach (var token in GetTokens(normalized))
        {
            if (token.Normalized.Length < 2 || !ProfanityTokens.Contains(token.Normalized))
                continue;

            ranges.Add((token.StartIndex, token.EndIndex));
        }

        var compact = BuildCompact(normalized, out var map);
        if (map.Length == 0 || ProfanityPhrases.Length == 0)
            return;

        foreach (var phrase in ProfanityPhrases)
        {
            var idx = compact.IndexOf(phrase, StringComparison.Ordinal);
            while (idx >= 0)
            {
                var end = idx + phrase.Length - 1;
                if (end < map.Length)
                    ranges.Add((map[idx], map[end]));

                idx = compact.IndexOf(phrase, idx + 1, StringComparison.Ordinal);
            }
        }
    }

    private static void AddRegexRanges(string text, Regex regex, List<(int, int)> ranges)
    {
        foreach (Match match in regex.Matches(text))
        {
            if (match.Length > 0)
                ranges.Add((match.Index, match.Index + match.Length - 1));
        }
    }

    private static void AddValidatedRanges(string text, Regex regex, List<(int, int)> ranges, int minDigits, int maxDigits, Func<string, bool>? validator)
    {
        foreach (Match match in regex.Matches(text))
        {
            if (match.Length <= 0)
                continue;

            var digits = CountDigits(match.Value);
            if (digits < minDigits || digits > maxDigits)
                continue;

            if (validator is not null && !validator(match.Value))
                continue;

            ranges.Add((match.Index, match.Index + match.Length - 1));
        }
    }

    private static List<(int Start, int End)> NormalizeRanges(IEnumerable<(int Start, int End)> ranges)
    {
        var sorted = ranges
            .Where(r => r.End >= r.Start)
            .OrderBy(r => r.Start)
            .ThenBy(r => r.End)
            .ToList();

        var result = new List<(int Start, int End)>();
        foreach (var (start, end) in sorted)
        {
            if (result.Count > 0 && start <= result[^1].End + 1)
            {
                var last = result[^1];
                result[^1] = (last.Start, Math.Max(last.End, end));
            }
            else
            {
                result.Add((start, end));
            }
        }

        return result;
    }

    private static int CountDigits(string value)
    {
        var count = 0;
        foreach (var ch in value)
        {
            if (ch is >= '0' and <= '9')
                count++;
        }

        return count;
    }

    private static string DigitsOnly(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch is >= '0' and <= '9')
                sb.Append(ch);
        }

        return sb.ToString();
    }

    private static bool ContainsValidCard(string text)
    {
        foreach (Match match in CreditCardRegex.Matches(text))
        {
            if (IsValidLuhn(DigitsOnly(match.Value)))
                return true;
        }

        return false;
    }

    private static bool IsValidLuhn(string digits)
    {
        var sum = 0;
        var doubleDigit = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleDigit)
            {
                d *= 2;
                if (d > 9)
                    d -= 9;
            }

            sum += d;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    private static char ToNormalLetter(char ch)
    {
        if (ch >= '0' && ch <= '9')
        {
            return ch switch
            {
                '0' => 'o',
                '1' => 'i',
                '2' => 'z',
                '4' => 'a',
                '5' => 's',
                '6' => 'g',
                '8' => 'b',
                '9' => 'g',
                _ => ch
            };
        }

        return char.IsLetter(ch) ? char.ToLowerInvariant(ch) : '\0';
    }

    private static string NormalizeCompact(string text)
    {
        var sb = new StringBuilder(text.Length);
        char? previous = null;
        foreach (var ch in text)
        {
            var c = ToNormalLetter(ch);
            if (c == '\0')
                continue;

            if (c == previous)
                continue;

            sb.Append(c);
            previous = c;
        }

        return sb.ToString();
    }

    private static List<NormChar> Normalize(string text)
    {
        var result = new List<NormChar>(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = ToNormalLetter(text[i]);
            if (c == '\0')
            {
                if (char.IsWhiteSpace(text[i]) && result.Count > 0 && result[^1].Char != ' ')
                    result.Add(new NormChar(' ', i));
                continue;
            }

            if (result.Count > 0 && result[^1].Char == c)
                continue;

            result.Add(new NormChar(c, i));
        }

        return result;
    }

    private static string BuildCompact(List<NormChar> normalized, out int[] indexMap)
    {
        var sb = new StringBuilder(normalized.Count);
        var map = new List<int>(normalized.Count);
        foreach (var item in normalized)
        {
            if (item.Char == ' ')
                continue;

            sb.Append(item.Char);
            map.Add(item.OriginalIndex);
        }

        indexMap = map.ToArray();
        return sb.ToString();
    }

    private static List<NormToken> GetTokens(List<NormChar> normalized)
    {
        var tokens = new List<NormToken>();
        var sb = new StringBuilder();
        var tokenStart = 0;

        for (var i = 0; i <= normalized.Count; i++)
        {
            if (i == normalized.Count || normalized[i].Char == ' ')
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new NormToken(sb.ToString(), tokenStart, normalized[i - 1].OriginalIndex));
                    sb.Clear();
                }

                continue;
            }

            if (sb.Length == 0)
                tokenStart = normalized[i].OriginalIndex;

            sb.Append(normalized[i].Char);
        }

        return tokens;
    }

    private static HashSet<string> BuildProfanityTokens()
    {
        var raw = new[]
        {
            "fuck", "fucking", "fucker", "fucks", "motherfucker", "motherfucking", "shit", "shitty", "shits",
            "bitch", "bitches", "bitchass", "bastard", "asshole", "assholes", "dick", "dickhead",
            "cunt", "cocksucker", "whore", "slut", "pussy", "prick", "twat", "wanker", "retard", "faggot",
            "fag", "nigger", "nigga", "moron", "idiot", "dumbass", "jackass", "douchebag", "bullshit",
            "fuckface", "fuckwit", "piss", "crap", "sonofabitch", "asshat", "fucktard",
            "f**k", "f***k", "fu*k", "s**t", "sh*t", "b***h", "b*tch", "d**k", "c**t", "a**hole",
            "p*ssy", "fck", "fuk", "fock", "shlt", "btch", "ahole", "dickhead",
            "كسم", "كسمك", "كس", "خول", "شرموط", "شرموطة", "متناك", "احا", "زب", "طيز", "عرص",
            "قحبة", "قحبه", "عاهرة", "خنزير", "كلب", "غبي", "حيوان", "متخلف", "وسخ", "نجس", "أحمق",
            "kosom", "kosomk", "ksmk", "5awal", "7omar", "metnak", "sharmota", "sharmoota", "3ars",
            "khawal", "zamel", "neek", "ahbal", "ghaby"
        };

        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in raw)
        {
            var normalized = NormalizeCompact(entry);
            if (normalized.Length >= 2)
                set.Add(normalized);
        }

        return set;
    }

    private static string[] BuildProfanityPhrases()
    {
        var phrases = new[]
        {
            "كس امك", "كس امها", "كسم امك", "يا ابن الكلب", "يا ابن الشرموطة", "ابن الكلب",
            "يا ابن الوسخة", "يا ابن كلب", "يا حيوان", "يا غبي", "يا عرص", "يا متخلف",
            "يا وسخ", "يا نجس", "يا احا",
            "kos omak", "kosom omak", "ya 7ayawan", "ya ahbal", "ya 3ars"
        };

        return phrases.Select(NormalizeCompact).ToArray();
    }

    private readonly struct NormChar
    {
        public readonly char Char;
        public readonly int OriginalIndex;

        public NormChar(char c, int originalIndex)
        {
            Char = c;
            OriginalIndex = originalIndex;
        }
    }

    private readonly struct NormToken
    {
        public readonly string Normalized;
        public readonly int StartIndex;
        public readonly int EndIndex;

        public NormToken(string normalized, int startIndex, int endIndex)
        {
            Normalized = normalized;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}
