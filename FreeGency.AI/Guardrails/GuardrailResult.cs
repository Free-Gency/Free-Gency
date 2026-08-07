namespace FreeGency.AI.Guardrails;

/// <summary>
/// The aggregate, content-agnostic result of running the guardrail engine over a
/// piece of text. Carries typed findings only: counts, kinds, and categories.
/// Raw values (the matched email, API key, phone number, and so on) are never
/// stored or exposed, so the result is safe to log and to cache.
/// </summary>
/// <param name="Language">The detected language, when language detection ran.</param>
/// <param name="PromptInjectionDetected">True when a prompt-injection attempt was detected.</param>
/// <param name="PromptInjectionKinds">The injection kinds that were detected.</param>
/// <param name="LlmAbuseDetected">True when an attempt to extract LLM internals or secrets was detected.</param>
/// <param name="LlmAbuseSignals">The abuse signals that were detected.</param>
/// <param name="SensitiveData">The sensitive value kinds that were detected.</param>
/// <param name="Profanity">Profanity findings grouped by writing style, with counts.</param>
/// <param name="Toxicity">The toxicity categories that were detected.</param>
/// <param name="Scams">The scam categories that were detected.</param>
/// <param name="AdvertisementDetected">True when promotional or external-contact content was detected.</param>
/// <param name="SpamSignals">The deterministic spam signals that were triggered.</param>
/// <param name="SpamRiskScore">The deterministic spam risk contribution on a 0..100 scale.</param>
public sealed record GuardrailResult(
    LanguageDetectionResult? Language,
    bool PromptInjectionDetected,
    IReadOnlyList<PromptInjectionKind> PromptInjectionKinds,
    bool LlmAbuseDetected,
    IReadOnlyList<string> LlmAbuseSignals,
    IReadOnlyList<SensitiveDataFinding> SensitiveData,
    IReadOnlyList<ProfanityFinding> Profanity,
    IReadOnlyList<ToxicityCategory> Toxicity,
    IReadOnlyList<ScamCategory> Scams,
    bool AdvertisementDetected,
    IReadOnlyList<SpamSignalType> SpamSignals,
    double SpamRiskScore)
{
    /// <summary>Gets whether any guardrail signal fired for the text.</summary>
    public bool HasAnySignal =>
        PromptInjectionDetected
        || LlmAbuseDetected
        || SensitiveData.Count > 0
        || Profanity.Count > 0
        || Toxicity.Count > 0
        || Scams.Count > 0
        || AdvertisementDetected
        || SpamSignals.Count > 0;

    /// <summary>The empty result returned for empty text or when guardrails are disabled.</summary>
    public static GuardrailResult None { get; } = new(
        null,
        false,
        [],
        false,
        [],
        [],
        [],
        [],
        [],
        false,
        [],
        0);
}

/// <summary>
/// A sensitive-value detection. Only the kind and how many were found are kept;
/// the matched value itself is never stored.
/// </summary>
/// <param name="Kind">The kind of sensitive value that was detected.</param>
/// <param name="Count">How many distinct matches were found.</param>
public sealed record SensitiveDataFinding(SensitiveDataKind Kind, int Count);

/// <summary>
/// Profanity findings grouped by the writing style they were detected in. Only
/// counts are kept, never the offending tokens.
/// </summary>
/// <param name="Style">The writing style (English, Arabic, Franco Arabic, masked, or leet).</param>
/// <param name="Count">How many distinct profanity hits were found.</param>
public sealed record ProfanityFinding(string Style, int Count);
