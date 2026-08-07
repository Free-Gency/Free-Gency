using FreeGency.AI.Guardrails;
using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Guardrails;

/// <summary>
/// The review-module view of a guardrail run. Maps the shared, content-agnostic
/// <see cref="GuardrailResult"/> onto the review contract: security categories,
/// extra keywords, a deterministic risk contribution, and the detected language.
/// The raw <see cref="Result"/> is retained for structured logging and metrics
/// (it only ever carries counts, kinds, and categories).
/// </summary>
/// <param name="Result">The shared guardrail result for the text.</param>
/// <param name="Language">The mapped review language, or null when undetermined.</param>
/// <param name="SecurityCategories">The review security categories contributed by the guardrails.</param>
/// <param name="ExtraKeywords">Stable keywords describing the guardrail findings.</param>
/// <param name="RiskContribution">The deterministic risk contribution on a 0..100 scale.</param>
public sealed record ReviewGuardrailOutcome(
    GuardrailResult Result,
    ReviewLanguage? Language,
    IReadOnlyList<ReviewSecurityCategory> SecurityCategories,
    IReadOnlyList<string> ExtraKeywords,
    double RiskContribution);
