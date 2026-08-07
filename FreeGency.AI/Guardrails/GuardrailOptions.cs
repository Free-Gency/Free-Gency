namespace FreeGency.AI.Guardrails;

/// <summary>
/// Options for the shared, content-agnostic guardrail engine. Bound to the
/// <c>AI:Guardrails</c> configuration section so every AI module (review, chat,
/// comment, portfolio, project, profile, proposal ranking, support tickets)
/// shares the same security posture. All values fall back to safe defaults when
/// the section is absent.
/// </summary>
public sealed record GuardrailOptions
{
    /// <summary>The configuration section name (<c>AI:Guardrails</c>).</summary>
    public const string SectionName = "AI:Guardrails";

    /// <summary>Gets or sets the master switch for the entire guardrail engine. Default <c>true</c>.</summary>
    public bool Enable { get; set; } = true;

    /// <summary>Gets or sets whether prompt-injection detection runs. Default <c>true</c>.</summary>
    public bool EnablePromptInjection { get; set; } = true;

    /// <summary>Gets or sets whether LLM-abuse detection runs. Default <c>true</c>.</summary>
    public bool EnableLlmAbuse { get; set; } = true;

    /// <summary>Gets or sets whether sensitive-data detection runs. Default <c>true</c>.</summary>
    public bool EnableSensitiveData { get; set; } = true;

    /// <summary>Gets or sets whether profanity detection runs. Default <c>true</c>.</summary>
    public bool EnableProfanity { get; set; } = true;

    /// <summary>Gets or sets whether toxicity detection runs. Default <c>true</c>.</summary>
    public bool EnableToxicity { get; set; } = true;

    /// <summary>Gets or sets whether scam detection runs. Default <c>true</c>.</summary>
    public bool EnableScam { get; set; } = true;

    /// <summary>Gets or sets whether advertisement detection runs. Default <c>true</c>.</summary>
    public bool EnableAdvertisement { get; set; } = true;

    /// <summary>Gets or sets whether spam detection runs. Default <c>true</c>.</summary>
    public bool EnableSpam { get; set; } = true;

    /// <summary>Gets or sets whether multi-language detection runs. Default <c>true</c>.</summary>
    public bool EnableLanguage { get; set; } = true;

    /// <summary>Gets or sets the text length below which a value is flagged as very short. Default 8.</summary>
    public int VeryShortLength { get; set; } = 8;

    /// <summary>Gets or sets the minimum number of repeated characters that flags character spam. Default 8.</summary>
    public int RepeatedCharThreshold { get; set; } = 8;

    /// <summary>Gets or sets the emoji count that flags emoji spam. Default 6.</summary>
    public int EmojiThreshold { get; set; } = 6;

    /// <summary>Gets or sets the unique-character ratio below which text is flagged as random. Default 0.15.</summary>
    public double LowDiversityThreshold { get; set; } = 0.15;

    /// <summary>Gets or sets the token count above which repeated-word detection activates. Default 12.</summary>
    public int MinTokensForRepeatedWords { get; set; } = 12;

    /// <summary>Gets or sets the share of identical tokens that flags repeated words. Default 0.40.</summary>
    public double RepeatedWordRatio { get; set; } = 0.40;

    /// <summary>Gets or sets the minimum normalized token length matched against the profanity list. Default 2.</summary>
    public int ProfanityMinTokenLength { get; set; } = 2;
}
