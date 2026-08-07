using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// The structured analysis of a review produced by the AI provider. It is the
/// provider contract; the service flattens it into a
/// <see cref="ReviewModerationResponse"/> and adds the approval decision and
/// processing telemetry. The full review understanding lives in
/// <see cref="Intelligence"/>, which is always produced (falling back to
/// <see cref="ReviewIntelligenceResult.None"/> when the AI is unavailable).
/// </summary>
public sealed record ReviewAnalysisResult(
    double RiskScore,
    double Confidence,
    RiskLevel RiskLevel,
    ReviewAction Action,
    double ToxicityScore,
    string? Reason,
    IReadOnlyList<ReviewCategoryDto> DetectedCategories,
    IReadOnlyList<ReviewSecurityCategory> SecurityCategories,
    IReadOnlyList<ReviewEntityDto> DetectedEntities,
    IReadOnlyList<string> DetectedKeywords,
    IReadOnlyList<ReviewSuggestionDto> Suggestions,
    string? MaskedReview,
    ReviewIntelligenceResult Intelligence,
    string PromptVersion,
    string? ModelName)
{
    /// <summary>
    /// Creates a safe fallback analysis that requires a human moderator. Used
    /// when the request is invalid or the AI service is unavailable.
    /// </summary>
    public static ReviewAnalysisResult ManualReview(
        string reason,
        string promptVersion,
        string? modelName)
        => new(
            50,
            0,
            RiskLevel.Medium,
            ReviewAction.ManualReview,
            0,
            reason,
            [],
            [],
            [],
            [],
            [],
            null,
            ReviewIntelligenceResult.None,
            promptVersion,
            modelName);
}
