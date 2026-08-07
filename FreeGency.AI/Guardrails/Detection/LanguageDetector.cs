namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic multi-language detector: English, Arabic, Franco Arabic
/// (Arabizi written with Latin letters and digits), and mixed script. Detects by
/// Unicode script coverage plus a curated Arabizi lexicon. No AI, no models.
/// </summary>
public sealed class LanguageDetector : IGuardrailDetector
{
    private const int MinScriptChars = 3;
    private const double MixedMinorityRatio = 0.25;
    private const double StrongRatio = 0.75;

    private static readonly HashSet<string> FrancoTokens = BuildFrancoTokens();

    /// <inheritdoc />
    public string Name => "Language";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableLanguage;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        var arabic = 0;
        var latin = 0;
        var other = 0;

        foreach (var ch in text)
        {
            if (GuardrailText.IsArabicScript(ch))
                arabic++;
            else if (GuardrailText.IsLatinScript(ch))
                latin++;
            else if (char.IsLetter(ch))
                other++;
        }

        var total = arabic + latin + other;
        if (total == 0)
        {
            builder.Language = new LanguageDetectionResult(GuardrailLanguage.Unknown, 0.5, false, false, 0);
            return;
        }

        var francoRatio = ComputeFrancoRatio(text);
        var hasArabic = arabic >= MinScriptChars;
        var hasLatin = latin >= MinScriptChars;

        GuardrailLanguage language;
        double confidence;

        if (other >= latin && other >= arabic)
        {
            language = GuardrailLanguage.Other;
            confidence = 0.7;
        }
        else if (hasArabic && hasLatin)
        {
            var arabicRatio = (double)arabic / (arabic + latin);
            if (arabicRatio >= StrongRatio)
            {
                language = GuardrailLanguage.Arabic;
                confidence = 0.9;
            }
            else if (arabicRatio >= MixedMinorityRatio)
            {
                language = GuardrailLanguage.Mixed;
                confidence = 0.9;
            }
            else
            {
                language = francoRatio >= 0.15 ? GuardrailLanguage.FrancoArabic : GuardrailLanguage.English;
                confidence = 0.8;
            }
        }
        else if (hasArabic)
        {
            language = GuardrailLanguage.Arabic;
            confidence = 0.95;
        }
        else if (hasLatin)
        {
            language = francoRatio >= 0.20 ? GuardrailLanguage.FrancoArabic : GuardrailLanguage.English;
            confidence = francoRatio >= 0.20 ? 0.75 : 0.8;
        }
        else
        {
            language = GuardrailLanguage.Unknown;
            confidence = 0.5;
        }

        builder.Language = new LanguageDetectionResult(
            language,
            confidence,
            arabic > 0,
            latin > 0,
            francoRatio);
    }

    private static double ComputeFrancoRatio(string text)
    {
        var tokens = GuardrailText.Tokenize(text);
        if (tokens.Count == 0)
            return 0;

        var matches = 0;
        foreach (var token in tokens)
        {
            var normalized = GuardrailText.Normalize(token);
            if (normalized.Length >= 2 && FrancoTokens.Contains(normalized))
                matches++;
        }

        return (double)matches / tokens.Count;
    }

    private static HashSet<string> BuildFrancoTokens()
    {
        var raw = new[]
        {
            "5awal", "7ayawan", "kosom", "kosomk", "ksmk", "sharmota", "sharmoota", "3ars",
            "ahbal", "ghaby", "zamel", "neek", "khawal", "habibi", "shukran", "sahbi", "sa7bi",
            "kolo", "bardo", "aywa", "3adi", "mashy", "kefak", "za3ma", "shlon", "laish",
            "wallah", "yalla", "tamam", "inshallah", "wayn", "fain", "3amal", "weza", "helo",
            "7elo", "zein", "ma3lesh", "bachi", "kbeer", "7arra", "sheel", "5allas"
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
}
