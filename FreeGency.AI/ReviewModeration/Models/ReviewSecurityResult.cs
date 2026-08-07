using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Guardrails;

namespace FreeGency.AI.ReviewModeration.Models;

/// <summary>
/// The result of the deterministic security analysis. It never depends on the AI
/// and is computed for every request, so security findings are available even
/// when the AI service is unavailable. Used internally by the review moderation
/// service; not returned to clients directly.
/// </summary>
/// <param name="SecurityCategories">The security categories detected deterministically.</param>
/// <param name="SpamSignals">The deterministic spam signals that were triggered.</param>
/// <param name="ExtraKeywords">Additional keywords derived from the deterministic findings.</param>
/// <param name="DeterministicRiskScore">The deterministic risk contribution on a 0..100 scale.</param>
/// <param name="MaskedText">The review text with profanity and sensitive values masked, or null when nothing was masked.</param>
/// <param name="Guardrails">The shared guardrail outcome (injection, abuse, toxicity, scam, advertisement, spam, language), or null when guardrails were disabled.</param>
public sealed record ReviewSecurityResult(
    IReadOnlyList<ReviewSecurityCategory> SecurityCategories,
    IReadOnlyList<string> SpamSignals,
    IReadOnlyList<string> ExtraKeywords,
    double DeterministicRiskScore,
    string? MaskedText,
    ReviewGuardrailOutcome? Guardrails = null);
