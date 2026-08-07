using Microsoft.Extensions.Options;

namespace FreeGency.AI.Guardrails;

/// <summary>
/// Runs the shared, deterministic guardrail engine over a piece of text. The
/// engine is content-agnostic and reusable by every AI module (review, chat,
/// comment, portfolio, project, profile, proposal ranking, support tickets).
/// It never calls the AI, never stores raw content, and never throws.
/// </summary>
public interface IGuardrailEngine
{
    /// <summary>
    /// Analyzes <paramref name="text"/> with every enabled detector and returns a
    /// single, typed <see cref="GuardrailResult"/>. Returns <see cref="GuardrailResult.None"/>
    /// for empty text or when guardrails are disabled.
    /// </summary>
    GuardrailResult Analyze(string? text, GuardrailDetectorContext? context = null);
}

/// <summary>
/// The default <see cref="IGuardrailEngine"/> implementation. Iterates the
/// registered detectors in registration order, running those enabled by
/// <see cref="GuardrailOptions"/>. A detector that throws is skipped so a defect
/// in one check can never take down the deterministic safety layer.
/// </summary>
public sealed class GuardrailEngine : IGuardrailEngine
{
    private readonly IReadOnlyList<IGuardrailDetector> _detectors;
    private readonly GuardrailOptions _options;

    public GuardrailEngine(IEnumerable<IGuardrailDetector> detectors, IOptions<GuardrailOptions> options)
    {
        _detectors = detectors.ToList();
        _options = options.Value;
    }

    /// <inheritdoc />
    public GuardrailResult Analyze(string? text, GuardrailDetectorContext? context = null)
    {
        var effective = context?.Options ?? _options;
        if (!effective.Enable || string.IsNullOrWhiteSpace(text))
            return GuardrailResult.None;

        var builder = new GuardrailResultBuilder();
        var ctx = context ?? new GuardrailDetectorContext();

        foreach (var detector in _detectors)
        {
            if (!detector.IsEnabled(effective))
                continue;

            try
            {
                detector.Analyze(text, ctx, builder);
            }
            catch
            {
                // The deterministic guardrail layer must never fail moderation.
            }
        }

        return builder.Build();
    }
}
