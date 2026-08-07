using FreeGency.AI.ReviewModeration.DTOs;

namespace FreeGency.AI.ReviewModeration.Guardrails;

/// <summary>
/// Maps the shared guardrail engine onto the review moderation contract. The
/// review pipeline runs this deterministically before any AI call so prompt
/// injection, abuse, toxicity, scam, and advertisement findings survive AI
/// outages and never depend on the model. Never stores raw text.
/// </summary>
public interface IReviewModerationGuardrails
{
    /// <summary>
    /// Analyzes <paramref name="text"/> with the shared guardrail engine and maps
    /// the findings onto the review contract. The previous reviews are used only
    /// for duplicate/copy-paste detection.
    /// </summary>
    ReviewGuardrailOutcome Analyze(string? text, IReadOnlyList<ReviewModerationRequest>? previousReviews);
}
