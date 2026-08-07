namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// The per-criterion quality assessment of a review. Every criterion is scored on
/// a 0..100 scale so the overall <see cref="AverageScore"/> stays comparable to
/// the review quality score. Nullable fields inside the raw model response are
/// resolved by the provider before this DTO is created.
/// </summary>
public sealed record ReviewQualityBreakdownDto(
    double Grammar,
    double Spelling,
    double Readability,
    double ProfessionalTone,
    double Constructiveness,
    double Helpfulness,
    double Clarity,
    double SpecificDetails,
    double Length,
    double Relevance,
    double Originality)
{
    /// <summary>The simple average of all eleven criteria, rounded to one decimal.</summary>
    public double AverageScore
        => Math.Round(
            (Grammar + Spelling + Readability + ProfessionalTone + Constructiveness
             + Helpfulness + Clarity + SpecificDetails + Length + Relevance + Originality) / 11,
            1);
}
