using FreeGency.AI.Guardrails;
using FreeGency.AI.ReviewModeration.Caching;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.ReviewModeration.Guardrails;

/// <summary>
/// The default <see cref="IReviewModerationGuardrails"/> implementation. Runs the
/// shared <see cref="IGuardrailEngine"/> over the review text and maps the
/// findings onto review security categories, keywords, a deterministic risk
/// contribution, and the detected language. The review-specific
/// <c>AI:ReviewModeration</c> flags override the shared <c>AI:Guardrails</c>
/// settings per module, so the review pipeline can independently enable or
/// disable individual detectors. Stateless and thread-safe.
/// </summary>
public sealed class ReviewModerationGuardrails : IReviewModerationGuardrails
{
    private const double PromptInjectionContribution = 90;
    private const double LlmAbuseContribution = 85;
    private const double ScamContribution = 85;
    private const double ViolenceContribution = 80;
    private const double ToxicityContribution = 60;
    private const double SensitiveContribution = 60;
    private const double ProfanityContribution = 55;
    private const double AdvertisementContribution = 40;

    private readonly IGuardrailEngine _engine;
    private readonly GuardrailOptions _sharedOptions;
    private readonly ReviewModerationCacheOptions _reviewOptions;

    public ReviewModerationGuardrails(
        IGuardrailEngine engine,
        IOptions<GuardrailOptions> sharedOptions,
        IOptions<ReviewModerationCacheOptions> reviewOptions)
    {
        _engine = engine;
        _sharedOptions = sharedOptions.Value;
        _reviewOptions = reviewOptions.Value;
    }

    /// <inheritdoc />
    public ReviewGuardrailOutcome Analyze(string? text, IReadOnlyList<ReviewModerationRequest>? previousReviews)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ReviewGuardrailOutcome(
                GuardrailResult.None,
                null,
                [],
                [],
                0);
        }

        var context = new GuardrailDetectorContext
        {
            PriorTexts = previousReviews?
                .Select(r => r.ReviewText)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList(),
            Options = BuildEffectiveOptions()
        };

        var result = _engine.Analyze(text, context);

        var categories = new HashSet<ReviewSecurityCategory>();
        var keywords = new List<string>();
        var contribution = 0d;

        if (result.PromptInjectionDetected)
        {
            categories.Add(ReviewSecurityCategory.PromptInjection);
            keywords.Add("prompt-injection");
            contribution = Math.Max(contribution, PromptInjectionContribution);
        }

        if (result.LlmAbuseDetected)
        {
            categories.Add(ReviewSecurityCategory.LlmAbuse);
            keywords.Add("llm-abuse");
            contribution = Math.Max(contribution, LlmAbuseContribution);
        }

        if (result.Scams.Count > 0)
        {
            categories.Add(ReviewSecurityCategory.Scam);
            keywords.Add("scam");
            contribution = Math.Max(contribution, ScamContribution);
        }

        if (result.Toxicity.Count > 0)
        {
            categories.Add(ReviewSecurityCategory.Toxicity);
            keywords.Add("toxicity");
            var severe = result.Toxicity.Any(c =>
                c is ToxicityCategory.Violence or ToxicityCategory.Threat or ToxicityCategory.Extremism);
            contribution = Math.Max(contribution, severe ? ViolenceContribution : ToxicityContribution);
        }

        if (result.SensitiveData.Count > 0)
        {
            categories.Add(ReviewSecurityCategory.SensitiveInformation);
            keywords.Add("sensitive-information");
            contribution = Math.Max(contribution, SensitiveContribution);
        }

        if (result.Profanity.Count > 0)
        {
            categories.Add(ReviewSecurityCategory.Profanity);
            keywords.Add("profanity");
            contribution = Math.Max(contribution, ProfanityContribution);
        }

        if (result.AdvertisementDetected)
        {
            categories.Add(ReviewSecurityCategory.Advertisement);
            keywords.Add("advertisement");
            contribution = Math.Max(contribution, AdvertisementContribution);
        }

        if (result.SpamSignals.Count > 0)
        {
            categories.Add(ReviewSecurityCategory.Spam);
            keywords.Add("spam");
            contribution = Math.Max(contribution, result.SpamRiskScore);
        }

        var language = MapLanguage(result.Language?.Language);

        return new ReviewGuardrailOutcome(
            result,
            language,
            categories.OrderBy(c => c).ToList(),
            keywords.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            Math.Round(contribution));
    }

    private static ReviewLanguage? MapLanguage(GuardrailLanguage? language)
        => language switch
        {
            GuardrailLanguage.English => ReviewLanguage.English,
            GuardrailLanguage.Arabic => ReviewLanguage.Arabic,
            GuardrailLanguage.FrancoArabic => ReviewLanguage.FrancoArabic,
            GuardrailLanguage.Mixed => ReviewLanguage.Mixed,
            _ => null
        };

    /// <summary>
    /// Builds the per-module guardrail options: the shared <c>AI:Guardrails</c>
    /// settings overridden by the review-specific <c>AI:ReviewModeration</c>
    /// flags wherever they are configured.
    /// </summary>
    private GuardrailOptions BuildEffectiveOptions()
        => _sharedOptions with
        {
            EnablePromptInjection = _reviewOptions.EnablePromptInjectionDetection ?? _sharedOptions.EnablePromptInjection,
            EnableSensitiveData = _reviewOptions.EnableSensitiveDataDetection ?? _sharedOptions.EnableSensitiveData,
            EnableSpam = _reviewOptions.EnableSpamDetection ?? _sharedOptions.EnableSpam,
            EnableScam = _reviewOptions.EnableScamDetection ?? _sharedOptions.EnableScam,
            EnableProfanity = _reviewOptions.EnableProfanityDetection ?? _sharedOptions.EnableProfanity,
            EnableLanguage = _reviewOptions.EnableMultiLanguageDetection ?? _sharedOptions.EnableLanguage
        };
}
