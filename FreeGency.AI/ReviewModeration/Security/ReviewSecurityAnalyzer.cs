using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Guardrails;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Security;

/// <summary>
/// Combines the deterministic content masker, the spam detector, and the shared
/// guardrail engine into one security analysis. Produces the deterministic
/// security categories, spam signals, extra keywords, a deterministic risk
/// contribution, the masked review text, and the guardrail findings. Never calls
/// the AI, so security findings survive AI outages. Stateless and thread-safe.
/// </summary>
public sealed class ReviewSecurityAnalyzer : IReviewSecurityAnalyzer
{
    private const double ProfanityScore = 55;
    private const double SensitiveScore = 60;

    private readonly IReviewContentMasker _masker;
    private readonly IReviewSpamDetector _spamDetector;
    private readonly IReviewModerationGuardrails _guardrails;

    public ReviewSecurityAnalyzer(
        IReviewContentMasker masker,
        IReviewSpamDetector spamDetector,
        IReviewModerationGuardrails guardrails)
    {
        _masker = masker;
        _spamDetector = spamDetector;
        _guardrails = guardrails;
    }

    /// <inheritdoc />
    public ReviewSecurityResult Analyze(string text, IReadOnlyList<ReviewModerationRequest>? previousReviews)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new ReviewSecurityResult([], [], [], 0, null);

        var categories = new HashSet<ReviewSecurityCategory>();
        var keywords = new List<string>();
        var spam = new ReviewSpamResult([], 0);
        var score = 0d;

        if (_masker.ContainsProfanity(text))
        {
            categories.Add(ReviewSecurityCategory.Profanity);
            keywords.Add("profanity");
            score = Math.Max(score, ProfanityScore);
        }

        var sensitive = _masker.FindSensitive(text);
        foreach (var match in sensitive)
        {
            categories.Add(match.Category);
            keywords.Add(match.Name.ToLowerInvariant());
            score = Math.Max(score, SensitiveScore);
        }

        spam = _spamDetector.Detect(text, previousReviews);
        if (spam.Signals.Count > 0)
        {
            categories.Add(ReviewSecurityCategory.Spam);
            keywords.Add("spam");
            score = Math.Max(score, spam.Score);
        }

        var guardrails = _guardrails.Analyze(text, previousReviews);
        foreach (var category in guardrails.SecurityCategories)
            categories.Add(category);

        foreach (var keyword in guardrails.ExtraKeywords)
            keywords.Add(keyword);

        score = Math.Max(score, guardrails.RiskContribution);

        var maskedText = _masker.Mask(text);

        return new ReviewSecurityResult(
            categories.OrderBy(c => c).ToList(),
            spam.Signals.Select(s => s.ToString()).ToList(),
            keywords.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            Math.Round(score),
            maskedText,
            guardrails);
    }
}
