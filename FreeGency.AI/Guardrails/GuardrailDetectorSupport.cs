namespace FreeGency.AI.Guardrails;

/// <summary>
/// Optional context passed to the guardrail engine alongside the text being
/// analyzed. Content-agnostic: any module can supply prior texts (for example a
/// reviewer's previous reviews) to enable duplicate-detection. Never carries
/// raw user content back out of the engine.
/// </summary>
public sealed class GuardrailDetectorContext
{
    /// <summary>Prior texts to compare against for duplicate/copy-paste detection.</summary>
    public IReadOnlyList<string>? PriorTexts { get; init; }

    /// <summary>
    /// Module-specific options override. When set, the engine gates every detector
    /// on this instance instead of the shared <see cref="GuardrailOptions"/>, so a
    /// module (for example review moderation) can independently switch individual
    /// detectors on or off. When null, the shared options are used.
    /// </summary>
    public GuardrailOptions? Options { get; init; }
}

/// <summary>
/// A single, self-contained guardrail check. Each detector inspects the text and
/// accumulates typed findings into the shared <see cref="GuardrailResultBuilder"/>.
/// Detectors are deterministic, stateless, and thread-safe. The engine runs every
/// enabled detector and never lets a detector exception escape.
/// </summary>
public interface IGuardrailDetector
{
    /// <summary>The stable, content-agnostic detector name (used in logs).</summary>
    string Name { get; }

    /// <summary>Returns whether this detector should run for the given options.</summary>
    bool IsEnabled(GuardrailOptions options);

    /// <summary>Analyzes the text and records findings into <paramref name="builder"/>.</summary>
    void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder);
}

/// <summary>
/// Accumulates typed findings from every enabled detector into a single
/// <see cref="GuardrailResult"/>. Only counts, kinds, and categories are retained;
/// matched values are never stored.
/// </summary>
public sealed class GuardrailResultBuilder
{
    private readonly Dictionary<PromptInjectionKind, int> _injections = new();
    private readonly List<string> _abuseSignals = [];
    private readonly Dictionary<SensitiveDataKind, int> _sensitive = new();
    private readonly Dictionary<string, int> _profanity = new();
    private readonly HashSet<ToxicityCategory> _toxicity = [];
    private readonly HashSet<ScamCategory> _scams = [];
    private readonly HashSet<SpamSignalType> _spamSignals = [];

    /// <summary>The detected language, set by the language detector.</summary>
    public LanguageDetectionResult? Language { get; set; }

    /// <summary>The aggregate spam risk contribution on a 0..100 scale.</summary>
    public double SpamRiskScore { get; set; }

    /// <summary>Records a detected prompt-injection kind.</summary>
    public void AddPromptInjection(PromptInjectionKind kind)
    {
        _injections[kind] = _injections.GetValueOrDefault(kind) + 1;
    }

    /// <summary>Records a detected LLM-abuse signal label.</summary>
    public void AddLlmAbuse(string signal)
    {
        if (!string.IsNullOrWhiteSpace(signal) && !_abuseSignals.Contains(signal, StringComparer.Ordinal))
            _abuseSignals.Add(signal);
    }

    /// <summary>Records a sensitive-value kind, accumulating the match count.</summary>
    public void AddSensitive(SensitiveDataKind kind, int count = 1)
    {
        _sensitive[kind] = _sensitive.GetValueOrDefault(kind) + Math.Max(1, count);
    }

    /// <summary>Records a profanity hit for a writing style, accumulating the count.</summary>
    public void AddProfanity(string style, int count = 1)
    {
        _profanity[style] = _profanity.GetValueOrDefault(style) + Math.Max(1, count);
    }

    /// <summary>Records a detected toxicity category.</summary>
    public void AddToxicity(ToxicityCategory category) => _toxicity.Add(category);

    /// <summary>Records a detected scam category.</summary>
    public void AddScam(ScamCategory category) => _scams.Add(category);

    /// <summary>Records a detected spam signal and the associated risk contribution.</summary>
    public void AddSpamSignal(SpamSignalType signal, double riskScore)
    {
        _spamSignals.Add(signal);
        SpamRiskScore = Math.Max(SpamRiskScore, riskScore);
    }

    /// <summary>Builds the final, immutable <see cref="GuardrailResult"/>.</summary>
    public GuardrailResult Build()
    {
        return new GuardrailResult(
            Language,
            _injections.Count > 0,
            _injections.Keys.OrderBy(k => (int)k).ToList(),
            _abuseSignals.Count > 0,
            _abuseSignals,
            _sensitive.OrderBy(k => (int)k.Key).Select(kv => new SensitiveDataFinding(kv.Key, kv.Value)).ToList(),
            _profanity.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => new ProfanityFinding(kv.Key, kv.Value)).ToList(),
            _toxicity.OrderBy(c => (int)c).ToList(),
            _scams.OrderBy(c => (int)c).ToList(),
            AdvertisementDetected,
            _spamSignals.OrderBy(s => (int)s).ToList(),
            Math.Round(SpamRiskScore));
    }

    /// <summary>Gets whether promotional or external-contact content was detected.</summary>
    public bool AdvertisementDetected { get; private set; }

    /// <summary>Marks the text as containing promotional or external-contact content.</summary>
    public void MarkAdvertisement() => AdvertisementDetected = true;
}
