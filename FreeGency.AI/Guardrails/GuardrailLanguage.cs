namespace FreeGency.AI.Guardrails;

/// <summary>
/// The dominant language detected by the guardrail engine. Content-agnostic so
/// the same detector serves review, chat, comment, and profile moderation.
/// </summary>
public enum GuardrailLanguage
{
    /// <summary>The language could not be determined.</summary>
    Unknown = 0,

    /// <summary>The text is predominantly English.</summary>
    English = 1,

    /// <summary>The text is predominantly Arabic.</summary>
    Arabic = 2,

    /// <summary>
    /// Arabic written with Latin letters and digits (Arabizi), for example
    /// "5awal", "ya 7ayawan", "kosomk".
    /// </summary>
    FrancoArabic = 3,

    /// <summary>The text mixes Arabic and Latin scripts or languages.</summary>
    Mixed = 4,

    /// <summary>The text is a language other than English, Arabic, or Franco Arabic.</summary>
    Other = 5
}

/// <summary>
/// The result of the deterministic language detection pass. Only script-level
/// signals and a confidence estimate are carried; no content is stored.
/// </summary>
/// <param name="Language">The detected language.</param>
/// <param name="Confidence">A 0..1 confidence estimate for the verdict.</param>
/// <param name="HasArabicScript">True when the text contains Arabic-script characters.</param>
/// <param name="HasLatinScript">True when the text contains Latin-script characters.</param>
/// <param name="FrancoTokenRatio">The share of tokens that look like Arabizi, on a 0..1 scale.</param>
public sealed record LanguageDetectionResult(
    GuardrailLanguage Language,
    double Confidence,
    bool HasArabicScript,
    bool HasLatinScript,
    double FrancoTokenRatio);
