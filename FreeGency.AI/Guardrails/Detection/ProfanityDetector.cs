namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic profanity detector covering English, Arabic, Franco Arabic, and
/// masked/leet spellings. Token and phrase matching happens on the canonical
/// normalized form (digits-to-letters, masking removal, repeated-character
/// collapsing), so "f**k", "fu*k", "fuck", and "فك" all resolve consistently.
/// Only counts grouped by writing style are recorded.
/// </summary>
public sealed class ProfanityDetector : IGuardrailDetector
{
    private static readonly HashSet<string> FrancoTokens = BuildFrancoSet();
    private static readonly Dictionary<string, string> ProfanityTokens = BuildProfanityTokens();
    private static readonly ProfanityPhrase[] ProfanityPhrases = BuildProfanityPhrases();

    /// <inheritdoc />
    public string Name => "Profanity";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableProfanity;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        var matched = new HashSet<string>(StringComparer.Ordinal);

        foreach (var token in GuardrailText.Tokenize(text))
        {
            var normalized = GuardrailText.Normalize(token);
            if (normalized.Length < 2 || !ProfanityTokens.TryGetValue(normalized, out var style))
                continue;

            if (matched.Add(normalized))
                builder.AddProfanity(style);
        }

        // Masked spellings (for example "f**k" or "sh*t") are split by the tokenizer
        // because asterisks are not word characters, so re-tokenize after stripping
        // the masking marks to resolve them onto the canonical profanity list.
        var deMasked = text.Replace("*", string.Empty, StringComparison.Ordinal);
        if (!string.Equals(deMasked, text, StringComparison.Ordinal))
        {
            foreach (var token in GuardrailText.Tokenize(deMasked))
            {
                var normalized = GuardrailText.Normalize(token);
                if (normalized.Length < 2 || !ProfanityTokens.TryGetValue(normalized, out var style))
                    continue;

                if (matched.Add(normalized))
                    builder.AddProfanity(style);
            }
        }

        var compact = GuardrailText.Compact(text);
        foreach (var phrase in ProfanityPhrases)
        {
            if (compact.Contains(phrase.Compacted, StringComparison.Ordinal) && matched.Add("phrase:" + phrase.Compacted))
                builder.AddProfanity(phrase.Style);
        }
    }

    private static string ClassifyStyle(string original, string normalized)
    {
        if (original.Any(ch => GuardrailText.IsArabicScript(ch)))
            return "Arabic";

        if (original.IndexOf('*') >= 0)
            return "Masked";

        if (original.Any(ch => ch is >= '0' and <= '9'))
            return "Leet";

        return FrancoTokens.Contains(normalized) ? "Franco" : "English";
    }

    private static HashSet<string> BuildFrancoSet()
    {
        var raw = new[]
        {
            "kosom", "kosomk", "ksmk", "sharmota", "sharmoota", "3ars", "ahbal", "ghaby",
            "zamel", "neek", "khawal", "5awal", "7ayawan", "metnak", "3ab", "z2i", "zib"
        };

        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in raw)
        {
            var normalized = GuardrailText.Normalize(entry);
            if (normalized.Length >= 2)
                set.Add(normalized);
        }

        return set;
    }

    private static Dictionary<string, string> BuildProfanityTokens()
    {
        var raw = new[]
        {
            // English
            "fuck", "fucking", "fucker", "fucks", "motherfucker", "motherfucking", "shit", "shitty", "shits",
            "bitch", "bitches", "bitchass", "bastard", "asshole", "assholes", "dick", "dickhead",
            "cunt", "cocksucker", "whore", "slut", "pussy", "prick", "twat", "wanker", "retard", "faggot",
            "fag", "nigger", "nigga", "moron", "idiot", "dumbass", "jackass", "douchebag", "bullshit",
            "fuckface", "fuckwit", "piss", "crap", "sonofabitch", "asshat", "fucktard",
            // Masked variants
            "f**k", "f***k", "fu*k", "s**t", "sh*t", "b***h", "b*tch", "d**k", "c**t", "a**hole",
            "p*ssy",
            // Leet variants
            "fck", "fuk", "fock", "shlt", "btch", "ahole",
            // Arabic
            "كسم", "كسمك", "كس", "خول", "شرموط", "شرموطة", "متناك", "احا", "زب", "طيز", "عرص",
            "قحبة", "قحبه", "عاهرة", "خنزير", "كلب", "غبي", "حيوان", "متخلف", "وسخ", "نجس", "أحمق",
            // Franco Arabic
            "kosom", "kosomk", "ksmk", "5awal", "7omar", "metnak", "sharmota", "sharmoota", "3ars",
            "khawal", "zamel", "neek", "ahbal", "ghaby"
        };

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in raw)
        {
            var normalized = GuardrailText.Normalize(entry);
            if (normalized.Length < 2)
                continue;

            if (!map.ContainsKey(normalized))
                map[normalized] = ClassifyStyle(entry, normalized);
        }

        return map;
    }

    private static ProfanityPhrase[] BuildProfanityPhrases()
    {
        var raw = new[]
        {
            "كس امك", "كس امها", "كسم امك", "يا ابن الكلب", "يا ابن الشرموطة", "ابن الكلب",
            "يا ابن الوسخة", "يا ابن كلب", "يا حيوان", "يا غبي", "يا عرص", "يا متخلف",
            "يا وسخ", "يا نجس", "يا احا",
            "kos omak", "kosom omak", "ya 7ayawan", "ya ahbal", "ya 3ars",
            "son of a bitch", "mother fucker", "fuck you", "fuck off", "eat shit",
            "shit head", "piece of shit", "ass hole", "kiss my ass", "suck my dick"
        };

        return raw
            .Select(entry => new ProfanityPhrase(
                GuardrailText.Compact(entry),
                entry.Any(ch => GuardrailText.IsArabicScript(ch)) ? "Arabic" : "English"))
            .ToArray();
    }

    private sealed record ProfanityPhrase(string Compacted, string Style);
}
