using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.Api.Contracts;

/// <summary>
/// Projects the service-level <see cref="ReviewModerationResponse"/> into the
/// frontend-facing <see cref="ReviewModerationApiResponse"/> contract. Keeps the
/// API contract and the AI service contract independent so either can evolve
/// without forcing a change on the other.
/// </summary>
public static class ReviewModerationApiResponseMapper
{
    /// <summary>
    /// Maps a moderation verdict to the frontend contract.
    /// </summary>
    /// <param name="response">The service-level moderation result.</param>
    /// <returns>The frontend-facing verdict.</returns>
    public static ReviewModerationApiResponse ToApiResponse(this ReviewModerationResponse response)
        => new()
        {
            Approved = response.Approved,
            RiskScore = response.RiskScore,
            Confidence = response.Confidence,
            RiskLevel = response.RiskLevel,
            Action = response.Action,
            Status = ToStatus(response.Action),
            Sentiment = response.Sentiment,
            SentimentConfidence = response.SentimentConfidence,
            QualityScore = response.QualityScore,
            QualityBand = response.QualityBand,
            AuthenticityScore = response.AuthenticityScore,
            ToxicityScore = response.ToxicityScore,
            Summary = response.Summary,
            Reason = response.Reason,
            DetectedCategories = response.DetectedCategories,
            SecurityCategories = response.SecurityCategories,
            SpamSignals = response.SpamSignals,
            DetectedEntities = response.DetectedEntities,
            SuggestedTags = response.SuggestedTags,
            Strengths = response.Strengths,
            Weaknesses = response.Weaknesses,
            Recommendation = response.Recommendation,
            DetectedKeywords = response.DetectedKeywords,
            Suggestions = response.Suggestions,
            MaskedReview = response.MaskedReview,
            ProcessingTime = response.ProcessingTime,
            Cached = response.FromCache,
            PromptVersion = response.PromptVersion,
            Model = response.ModelName
        };

    /// <summary>
    /// Maps the recommended enforcement action to the frontend display status.
    /// </summary>
    /// <param name="action">The recommended action.</param>
    /// <returns>
    /// "Safe" for <see cref="ReviewAction.Allow"/>, "Warning" for
    /// <see cref="ReviewAction.Warn"/>, "Masked" for <see cref="ReviewAction.Mask"/>,
    /// "Rejected" for <see cref="ReviewAction.Reject"/>, and "Manual Review" for
    /// <see cref="ReviewAction.ManualReview"/>.
    /// </returns>
    public static string ToStatus(ReviewAction action) => action switch
    {
        ReviewAction.Allow => "Safe",
        ReviewAction.Warn => "Warning",
        ReviewAction.Mask => "Masked",
        ReviewAction.Reject => "Rejected",
        _ => "Manual Review"
    };
}
