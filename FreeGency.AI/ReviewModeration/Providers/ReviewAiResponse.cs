namespace FreeGency.AI.ReviewModeration.Providers;

/// <summary>
/// The raw JSON shape the review moderation model returns. All fields are
/// optional and are mapped defensively by <see cref="ReviewModerationJsonParser"/>.
/// </summary>
public sealed class ReviewAiResponse
{
    public double? RiskScore { get; set; }
    public double? Confidence { get; set; }
    public string? RiskLevel { get; set; }
    public string? Action { get; set; }
    public string? Sentiment { get; set; }
    public double? SentimentConfidence { get; set; }
    public double? QualityScore { get; set; }
    public ReviewRawQualityBreakdown? QualityBreakdown { get; set; }
    public bool? Constructive { get; set; }
    public double? ConstructivenessScore { get; set; }
    public double? AuthenticityScore { get; set; }
    public string? RatingConsistency { get; set; }
    public string? LengthCategory { get; set; }
    public string? WritingStyle { get; set; }
    public string? Language { get; set; }
    public double? Toxicity { get; set; }
    public ReviewRawSummary? Summary { get; set; }
    public string? FlatSummary { get; set; }
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
    public List<string> SecurityCategories { get; set; } = [];
    public string? Reason { get; set; }
    public List<ReviewRawCategory> Categories { get; set; } = [];
    public List<ReviewRawEntity> Entities { get; set; } = [];
    public List<string> SuggestedTags { get; set; } = [];
    public List<ReviewRawSuggestion> Suggestions { get; set; } = [];
    public string? Recommendation { get; set; }
    public string? MaskedReview { get; set; }
}

/// <summary>Raw summary object returned by the model.</summary>
public sealed record ReviewRawSummary(
    string? ShortSummary,
    IReadOnlyList<string>? PositivePoints,
    IReadOnlyList<string>? NegativePoints,
    IReadOnlyList<string>? KeyTopics);

/// <summary>
/// Raw per-criterion quality scores returned by the model. Every criterion is an
/// optional 0..100 number so a partial response still parses.
/// </summary>
public sealed record ReviewRawQualityBreakdown(
    double? Grammar,
    double? Spelling,
    double? Readability,
    double? ProfessionalTone,
    double? Constructiveness,
    double? Helpfulness,
    double? Clarity,
    double? SpecificDetails,
    double? Length,
    double? Relevance,
    double? Originality);

/// <summary>Raw category object returned by the model.</summary>
public sealed record ReviewRawCategory(string Category, double? Score);

/// <summary>Raw entity object returned by the model.</summary>
public sealed record ReviewRawEntity(string Type, string Value, double? Confidence);

/// <summary>Raw suggestion object returned by the model.</summary>
public sealed record ReviewRawSuggestion(string Type, string Message, string? Severity);
