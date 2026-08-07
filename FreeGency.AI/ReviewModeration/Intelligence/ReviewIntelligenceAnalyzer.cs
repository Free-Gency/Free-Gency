using System.Text;
using System.Text.RegularExpressions;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Intelligence;

/// <summary>
/// Deterministic review intelligence. Reads the review text with heuristics and
/// lightweight lexicons to produce sentiment, quality, constructiveness,
/// authenticity, rating consistency, length, writing style, language, a short
/// summary, strengths, weaknesses, tags, and a recommendation. It never calls the
/// AI, so the understanding survives AI outages. Stateless and thread-safe.
/// </summary>
public sealed class ReviewIntelligenceAnalyzer : IReviewIntelligenceAnalyzer
{
    private const int MaxTags = 7;
    private const int MinTags = 3;
    private const int MaxAspects = 5;

    private static readonly Regex SentenceSplitRegex = new(@"[.!؟?;؛\n\r]+", RegexOptions.Compiled);
    private static readonly Regex TokenRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);

    private readonly IReviewContentMasker _masker;
    private readonly IReviewSpamDetector _spamDetector;

    public ReviewIntelligenceAnalyzer(IReviewContentMasker masker, IReviewSpamDetector spamDetector)
    {
        _masker = masker;
        _spamDetector = spamDetector;
    }

    /// <inheritdoc />
    public ReviewIntelligenceResult Analyze(
        string text,
        int? rating,
        IReadOnlyList<ReviewModerationRequest>? previousReviews)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ReviewIntelligenceResult.None;

        var spam = _spamDetector.Detect(text, previousReviews);
        var language = DetectLanguage(text);
        var lengthCategory = ClassifyLength(text);
        var sentiment = DetectSentiment(text, out var sentimentConfidence);
        var ratingConsistency = CheckRatingConsistency(rating, sentiment);
        var specificCount = CountSpecifics(text, out var matchedAspects);
        var constructivenessScore = ComputeConstructiveness(text, lengthCategory, specificCount);
        var constructive = constructivenessScore >= 50;
        var writingStyle = DetectWritingStyle(text);
        var qualityScore = ComputeQualityScore(
            text, lengthCategory, specificCount, constructivenessScore, spam.Signals);
        var qualityBreakdown = BuildQualityBreakdown(
            text, lengthCategory, specificCount, constructivenessScore, writingStyle, spam.Signals);
        var qualityBand = MapQualityBand(qualityScore);
        var authenticityScore = ComputeAuthenticityScore(
            text, lengthCategory, specificCount, ratingConsistency, spam);
        var summary = BuildSummary(text);
        var (strengths, weaknesses) = ExtractAspects(text, sentiment, matchedAspects);
        var tags = BuildTags(sentiment, qualityBand, lengthCategory, matchedAspects, strengths, weaknesses);
        var recommendation = Recommend(qualityScore, authenticityScore, sentiment, ratingConsistency, constructive, spam);

        return new ReviewIntelligenceResult(
            sentiment,
            sentimentConfidence,
            qualityScore,
            qualityBand,
            qualityBreakdown,
            constructive,
            constructivenessScore,
            authenticityScore,
            ratingConsistency,
            lengthCategory,
            writingStyle,
            language,
            summary,
            strengths,
            weaknesses,
            tags,
            recommendation);
    }

    private static ReviewLanguage DetectLanguage(string text)
    {
        var arabic = 0;
        var latin = 0;
        var digits = 0;

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
                continue;

            if (ch >= 0x0600 && ch <= 0x06FF || ch >= 0x0750 && ch <= 0x077F || ch >= 0x08A0 && ch <= 0x08FF)
                arabic++;
            else if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z')
                latin++;
            else if (char.IsDigit(ch))
                digits++;
        }

        if (arabic == 0 && latin == 0)
            return ReviewLanguage.Unknown;

        if (arabic == 0)
            return digits > 0 && HasFrancoPattern(text)
                ? ReviewLanguage.FrancoArabic
                : ReviewLanguage.English;

        if (latin == 0)
            return ReviewLanguage.Arabic;

        var latinRatio = (double)latin / Math.Max(1, latin + arabic);
        if (latinRatio > 0.6)
            return HasFrancoPattern(text) ? ReviewLanguage.FrancoArabic : ReviewLanguage.Mixed;

        return ReviewLanguage.Mixed;
    }

    private static bool HasFrancoPattern(string text)
    {
        if (Regex.IsMatch(text, @"[\p{L}][23579][\p{L}]"))
            return true;

        var lower = text.ToLowerInvariant();
        var markers = new[]
        {
            "kosomk", "kosom", "koss", "5awal", "khawal", "7omar", "7ayawan", "metnak",
            "sharmota", "3ars", "enta", "enti", "entou", "ana", "aywa", "la2", "mashy",
            "3ady", "3ashan", "feen", "kolo", "kaman", "wayed", "kbeer", "sghayer",
            "ghaby", "3amal", "hayek", "zeft", "nazif", "kos"
        };

        return markers.Any(marker => lower.Contains(marker, StringComparison.Ordinal));
    }

    private static ReviewLengthCategory ClassifyLength(string text)
    {
        var length = text.Trim().Length;
        return length switch
        {
            < 20 => ReviewLengthCategory.VeryShort,
            < 50 => ReviewLengthCategory.Short,
            < 200 => ReviewLengthCategory.Normal,
            < 500 => ReviewLengthCategory.Detailed,
            _ => ReviewLengthCategory.VeryDetailed
        };
    }

    private static readonly (string Word, double Weight)[] SentimentWords =
    {
        ("excellent", 2.0), ("amazing", 2.0), ("awesome", 2.0), ("perfect", 2.0),
        ("perfectly", 2.0), ("fantastic", 2.0), ("wonderful", 2.0), ("outstanding", 2.0),
        ("superb", 2.0), ("brilliant", 2.0), ("impressive", 1.5), ("impressed", 1.5),
        ("best", 1.5), ("great", 1.2), ("love", 1.5), ("loved", 1.5), ("happy", 1.2),
        ("recommended", 1.2), ("recommend", 1.2), ("satisfied", 1.2), ("nice", 1.0),
        ("good", 1.0), ("well", 0.8), ("professional", 1.0), ("professionally", 1.0),
        ("reliable", 1.2), ("responsive", 1.2), ("helpful", 1.2), ("honest", 1.0),
        ("smooth", 0.9), ("fast", 0.8), ("quick", 0.8), ("quality", 0.6),
        ("delivered", 0.7), ("delivery", 0.5), ("creative", 1.0), ("clean", 0.8),
        ("detailed", 0.8), ("organized", 0.8), ("satisfied", 1.2), ("improved", 0.8),

        ("terrible", -2.0), ("horrible", -2.0), ("awful", -2.0), ("worst", -2.0),
        ("disgusting", -2.0), ("atrocious", -2.0), ("hate", -1.8), ("hated", -1.8),
        ("scam", -2.0), ("fraud", -2.0), ("waste", -1.5), ("useless", -1.8),
        ("sucks", -2.0), ("suck", -1.5), ("disappointed", -1.5), ("disappointing", -1.5),
        ("unprofessional", -1.8), ("rude", -1.8), ("arrogant", -1.5), ("slow", -1.0),
        ("late", -1.0), ("delay", -1.0), ("delayed", -1.2), ("bad", -1.2),
        ("poor", -1.3), ("worse", -1.4), ("buggy", -1.3), ("broken", -1.3),
        ("failed", -1.3), ("failure", -1.3), ("refund", -1.2), ("disappeared", -1.5),
        ("ghosted", -1.5), ("mediocre", -1.0), ("amateur", -1.2), ("overpriced", -1.0),

        ("ممتاز", 2.0), ("ممتازة", 2.0), ("مذهل", 2.0), ("مذهلة", 2.0), ("رائع", 2.0),
        ("رائعة", 2.0), ("مبدع", 1.5), ("مبدعة", 1.5), ("جميل", 1.5), ("جميلة", 1.5),
        ("جيد", 1.0), ("جيدة", 1.0), ("حلو", 1.0), ("محترم", 1.2), ("محترمة", 1.2),
        ("محترف", 1.2), ("محترفة", 1.2), ("انصح", 1.5), ("انصحك", 1.5), ("سعيد", 1.2),
        ("سعيدة", 1.2), ("ممتازة جدا", 2.0), ("احسن", 1.5), ("افضل", 1.5),

        ("سيئ", -1.5), ("سيئة", -1.5), ("فظيع", -2.0), ("فظيعة", -2.0), ("فاشل", -2.0),
        ("فاشلة", -2.0), ("مخيب", -1.5), ("مخيبة", -1.5), ("خداع", -2.0), ("نصب", -2.0),
        ("غش", -2.0), ("مؤجل", -1.2), ("متأخر", -1.2), ("بطيء", -1.0), ("بطيئة", -1.0),
        ("غاضب", -1.2), ("كذب", -2.0), ("كاذب", -2.0),

        ("kosomk", -2.5), ("kosom", -2.0), ("5awal", -2.0), ("khawal", -2.0),
        ("7omar", -2.0), ("sharmota", -2.5), ("metnak", -2.5), ("ghaby", -1.8),
        ("zeft", -1.8), ("3ars", -2.0), ("hayek", -1.5), ("7ayawan", -2.0)
    };

    private static ReviewSentiment DetectSentiment(string text, out double confidence)
    {
        var tokens = TokenList(text);
        if (tokens.Count == 0)
        {
            confidence = 0;
            return ReviewSentiment.Neutral;
        }

        var score = 0.0;
        var matches = 0;
        foreach (var token in tokens)
        {
            foreach (var (word, weight) in SentimentWords)
            {
                if (string.Equals(token, word, StringComparison.OrdinalIgnoreCase))
                {
                    score += weight;
                    matches++;
                    break;
                }
            }
        }

        if (matches == 0)
        {
            confidence = 0;
            return ReviewSentiment.Neutral;
        }

        var average = score / Math.Sqrt(matches);
        var sentiment = average switch
        {
            > 1.5 => ReviewSentiment.VeryPositive,
            > 0.4 => ReviewSentiment.Positive,
            < -1.5 => ReviewSentiment.VeryNegative,
            < -0.4 => ReviewSentiment.Negative,
            _ => ReviewSentiment.Neutral
        };

        confidence = Math.Clamp(0.3 + (matches * 0.12) + Math.Min(0.4, Math.Abs(average) / 3), 0, 1);
        return sentiment;
    }

    private static ReviewRatingConsistency CheckRatingConsistency(int? rating, ReviewSentiment sentiment)
    {
        if (rating is not int value)
            return ReviewRatingConsistency.Unknown;

        var positive = sentiment is ReviewSentiment.VeryPositive or ReviewSentiment.Positive;
        var negative = sentiment is ReviewSentiment.VeryNegative or ReviewSentiment.Negative;

        if (value >= 4 && negative)
            return ReviewRatingConsistency.Inconsistent;

        if (value <= 2 && positive)
            return ReviewRatingConsistency.Inconsistent;

        return ReviewRatingConsistency.Consistent;
    }

    private static double ComputeConstructiveness(string text, ReviewLengthCategory length, int specificCount)
    {
        var score = length switch
        {
            ReviewLengthCategory.VeryShort => 8,
            ReviewLengthCategory.Short => 18,
            ReviewLengthCategory.Normal => 28,
            ReviewLengthCategory.Detailed => 36,
            _ => 40
        };

        score += Math.Min(35, specificCount * 12);
        score += SentenceSplitRegex.Split(text).Count(IsNonEmpty) switch
        {
            >= 3 => 15,
            2 => 10,
            1 => 5,
            _ => 0
        };

        return Math.Clamp(score, 0, 100);
    }

    private static int CountSpecifics(string text, out List<string> matchedAspects)
    {
        var count = 0;

        if (DigitRegex.IsMatch(text))
            count++;

        matchedAspects = Aspects
            .Where(a => text.Contains(a.Word, StringComparison.OrdinalIgnoreCase))
            .Select(a => a.Aspect)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        count += Math.Min(3, matchedAspects.Count);
        count += text.Length >= 80 ? 1 : 0;

        return count;
    }

    private static double ComputeQualityScore(
        string text,
        ReviewLengthCategory length,
        int specificCount,
        double constructivenessScore,
        IReadOnlyList<ReviewSpamSignal> spamSignals)
    {
        double score = length switch
        {
            ReviewLengthCategory.VeryShort => 25,
            ReviewLengthCategory.Short => 38,
            ReviewLengthCategory.Normal => 55,
            ReviewLengthCategory.Detailed => 68,
            _ => 72
        };

        score += Math.Min(20, specificCount * 5);
        score += Math.Min(10, constructivenessScore / 10);

        if (spamSignals.Contains(ReviewSpamSignal.DuplicateReview)
            || spamSignals.Contains(ReviewSpamSignal.CopyPaste))
            score -= 30;

        return Math.Clamp(Math.Round(score), 0, 100);
    }

    private static double ComputeAuthenticityScore(
        string text,
        ReviewLengthCategory length,
        int specificCount,
        ReviewRatingConsistency ratingConsistency,
        ReviewSpamResult spam)
    {
        if (spam.Signals.Contains(ReviewSpamSignal.DuplicateReview))
            return 15;

        var score = 65d;
        score += length switch
        {
            ReviewLengthCategory.VeryShort => -25,
            ReviewLengthCategory.Short => -10,
            ReviewLengthCategory.Detailed => 10,
            ReviewLengthCategory.VeryDetailed => 15,
            _ => 0
        };

        score += Math.Min(15, specificCount * 4);
        score += ratingConsistency == ReviewRatingConsistency.Inconsistent ? -15 : 5;

        if (spam.Signals.Any(s => s is not ReviewSpamSignal.VeryShort))
            score -= 20;

        if (text.Length >= 40)
            score += 5;

        return Math.Clamp(Math.Round(score), 0, 100);
    }

    private static ReviewWritingStyle DetectWritingStyle(string text)
    {
        if (text.Contains("please", StringComparison.OrdinalIgnoreCase)
            || text.Contains("kindly", StringComparison.OrdinalIgnoreCase)
            || text.Contains("however", StringComparison.OrdinalIgnoreCase)
            || text.Contains("regarding", StringComparison.OrdinalIgnoreCase))
            return ReviewWritingStyle.Formal;

        if (text.Count(ch => ch == '!') >= 2)
            return ReviewWritingStyle.Friendly;

        if (text.Length >= 120 && text.Count(ch => ch == ',') >= 3)
            return ReviewWritingStyle.Professional;

        if (text.Contains("pls", StringComparison.OrdinalIgnoreCase)
            || text.Contains("thx", StringComparison.OrdinalIgnoreCase)
            || text.Contains("omg", StringComparison.OrdinalIgnoreCase)
            || text.Contains("lol", StringComparison.OrdinalIgnoreCase))
            return ReviewWritingStyle.Informal;

        return ReviewWritingStyle.Casual;
    }

    private static ReviewQualityBand MapQualityBand(double score)
    {
        return score switch
        {
            >= 90 => ReviewQualityBand.Excellent,
            >= 70 => ReviewQualityBand.Good,
            >= 50 => ReviewQualityBand.Average,
            >= 30 => ReviewQualityBand.Poor,
            _ => ReviewQualityBand.VeryPoor
        };
    }

    private static ReviewQualityBreakdownDto? BuildQualityBreakdown(
        string text,
        ReviewLengthCategory length,
        int specificCount,
        double constructivenessScore,
        ReviewWritingStyle style,
        IReadOnlyList<ReviewSpamSignal> spamSignals)
    {
        var hasProfanity = text.Contains("fuck", StringComparison.OrdinalIgnoreCase)
            || text.Contains("shit", StringComparison.OrdinalIgnoreCase)
            || text.Contains("kosom", StringComparison.OrdinalIgnoreCase)
            || text.Contains("metnak", StringComparison.OrdinalIgnoreCase)
            || text.Contains("sharmota", StringComparison.OrdinalIgnoreCase);

        var duplicate = spamSignals.Contains(ReviewSpamSignal.DuplicateReview)
            || spamSignals.Contains(ReviewSpamSignal.CopyPaste);

        var grammar = Math.Clamp(78 - (hasProfanity ? 25 : 0) - (duplicate ? 40 : 0), 0, 100);
        var spelling = Math.Clamp(82 - (duplicate ? 40 : 0), 0, 100);
        var readability = length switch
        {
            ReviewLengthCategory.VeryShort => 92,
            ReviewLengthCategory.Short => 88,
            ReviewLengthCategory.Normal => 82,
            ReviewLengthCategory.Detailed => 74,
            _ => 62
        };
        var professionalTone = style == ReviewWritingStyle.Aggressive ? 15
            : hasProfanity ? 20
            : style == ReviewWritingStyle.Formal || style == ReviewWritingStyle.Professional ? 88
            : 75;
        var constructiveness = Math.Round(constructivenessScore);
        var helpfulness = Math.Round(constructivenessScore);
        var clarity = Math.Clamp(80 + (length is ReviewLengthCategory.Short or ReviewLengthCategory.Normal ? 10 : 0) - (hasProfanity ? 20 : 0), 0, 100);
        var specificDetails = Math.Clamp(specificCount * 22, 0, 100);
        var lengthScore = length switch
        {
            ReviewLengthCategory.VeryShort => 20,
            ReviewLengthCategory.Short => 45,
            ReviewLengthCategory.Normal => 75,
            ReviewLengthCategory.Detailed => 90,
            _ => 100
        };
        var relevance = 82d;
        var originality = duplicate ? 15 : 85;

        return new ReviewQualityBreakdownDto(
            grammar, spelling, readability, professionalTone, constructiveness,
            helpfulness, clarity, specificDetails, lengthScore, relevance, originality);
    }

    private static readonly (string Word, string Aspect, string Strength, string Weakness)[] Aspects =
    {
        ("communication", "Communication", "Strong Communication", "Weak Communication"),
        ("communicated", "Communication", "Strong Communication", "Weak Communication"),
        ("delivery", "Delivery", "Fast Delivery", "Late Delivery"),
        ("delivered", "Delivery", "Fast Delivery", "Late Delivery"),
        ("deadline", "Deadline Adherence", "Met the Deadline", "Missed the Deadline"),
        ("on time", "Deadline Adherence", "On-Time Delivery", "Missed the Deadline"),
        ("quality", "Quality", "High Quality", "Poor Quality"),
        ("design", "Design", "Excellent Design", "Poor Design"),
        ("code", "Code Quality", "Good Code Quality", "Poor Code Quality"),
        ("bug", "Code Quality", "Few Bugs", "Buggy"),
        ("bugs", "Code Quality", "Few Bugs", "Buggy"),
        ("documentation", "Documentation", "Good Documentation", "Poor Documentation"),
        ("documented", "Documentation", "Good Documentation", "Poor Documentation"),
        ("responsive", "Responsiveness", "Responsive", "Unresponsive"),
        ("support", "Support", "Helpful Support", "Poor Support"),
        ("professional", "Professionalism", "Professional", "Unprofessional"),
        ("creative", "Creativity", "Creative", "Uncreative"),
        ("price", "Pricing", "Fair Pricing", "Overpriced"),
        ("cost", "Pricing", "Fair Pricing", "Overpriced"),
        ("speed", "Speed", "Fast", "Slow"),
        ("fast", "Speed", "Fast", "Slow"),
        ("slow", "Speed", "Fast", "Slow"),
        ("requirements", "Requirements", "Covered Requirements", "Missing Requirements"),
        ("reliable", "Reliability", "Reliable", "Unreliable"),
        ("honest", "Trustworthiness", "Honest", "Dishonest"),
        ("clean", "Code Quality", "Clean Code", "Messy Code"),
        ("detailed", "Attention to Detail", "Detailed", "Lacked Detail"),
        ("organized", "Organization", "Organized", "Disorganized")
    };

    private static (List<string> Strengths, List<string> Weaknesses) ExtractAspects(
        string text,
        ReviewSentiment sentiment,
        IReadOnlyList<string> matchedAspects)
    {
        var strengths = new List<string>();
        var weaknesses = new List<string>();
        var sentences = SentenceSplitRegex.Split(text).Where(IsNonEmpty).ToList();

        foreach (var sentence in sentences)
        {
            var polarity = SentencePolarity(sentence);
            if (polarity == 0)
                continue;

            foreach (var (word, _, strength, weakness) in Aspects)
            {
                if (!sentence.Contains(word, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (polarity > 0)
                    AddDistinct(strengths, strength);
                else
                    AddDistinct(weaknesses, weakness);
            }
        }

        if (strengths.Count == 0 && matchedAspects.Count == 0
            && sentiment is ReviewSentiment.VeryPositive or ReviewSentiment.Positive)
        {
            strengths.Add("Positive Experience");
        }

        if (weaknesses.Count == 0 && matchedAspects.Count == 0
            && sentiment is ReviewSentiment.VeryNegative or ReviewSentiment.Negative)
        {
            weaknesses.Add("Negative Experience");
        }

        if (strengths.Count > MaxAspects)
            strengths = strengths.Take(MaxAspects).ToList();

        if (weaknesses.Count > MaxAspects)
            weaknesses = weaknesses.Take(MaxAspects).ToList();

        return (strengths, weaknesses);
    }

    private static List<string> BuildTags(
        ReviewSentiment sentiment,
        ReviewQualityBand qualityBand,
        ReviewLengthCategory length,
        IReadOnlyList<string> matchedAspects,
        IReadOnlyList<string> strengths,
        IReadOnlyList<string> weaknesses)
    {
        var tags = new List<string>();

        foreach (var aspect in matchedAspects)
        {
            var strength = strengths.FirstOrDefault(s =>
                s.EndsWith(aspect, StringComparison.OrdinalIgnoreCase));
            if (strength is not null)
                AddDistinct(tags, strength);

            var weakness = weaknesses.FirstOrDefault(w =>
                w.EndsWith(aspect, StringComparison.OrdinalIgnoreCase));
            if (weakness is not null)
                AddDistinct(tags, weakness);
        }

        switch (sentiment)
        {
            case ReviewSentiment.VeryPositive:
                AddDistinct(tags, "Very Positive");
                break;
            case ReviewSentiment.Positive:
                AddDistinct(tags, "Positive");
                break;
            case ReviewSentiment.Negative:
                AddDistinct(tags, "Negative");
                break;
            case ReviewSentiment.VeryNegative:
                AddDistinct(tags, "Very Negative");
                break;
        }

        switch (qualityBand)
        {
            case ReviewQualityBand.Excellent:
                AddDistinct(tags, "High Quality");
                AddDistinct(tags, "Recommended");
                break;
            case ReviewQualityBand.Good:
                AddDistinct(tags, "Recommended");
                break;
            case ReviewQualityBand.Poor:
            case ReviewQualityBand.VeryPoor:
                AddDistinct(tags, "Needs Improvement");
                break;
        }

        switch (length)
        {
            case ReviewLengthCategory.VeryDetailed:
            case ReviewLengthCategory.Detailed:
                AddDistinct(tags, "Detailed");
                break;
            case ReviewLengthCategory.VeryShort:
                AddDistinct(tags, "Brief");
                break;
        }

        var fallbacks = new[] { "Informative", "Genuine", "Client Review" };
        foreach (var fallback in fallbacks)
        {
            if (tags.Count >= MinTags)
                break;
            AddDistinct(tags, fallback);
        }

        if (tags.Count > MaxTags)
            tags = tags.Take(MaxTags).ToList();

        return tags;
    }

    private static ReviewRecommendation Recommend(
        double qualityScore,
        double authenticityScore,
        ReviewSentiment sentiment,
        ReviewRatingConsistency ratingConsistency,
        bool constructive,
        ReviewSpamResult spam)
    {
        if (spam.Signals.Contains(ReviewSpamSignal.DuplicateReview))
            return ReviewRecommendation.Reject;

        if (authenticityScore < 30)
            return ReviewRecommendation.ManualReview;

        if (qualityScore < 30)
            return ReviewRecommendation.Improve;

        if (qualityScore < 60 || ratingConsistency == ReviewRatingConsistency.Inconsistent)
            return ReviewRecommendation.Improve;

        if (sentiment == ReviewSentiment.VeryNegative || !constructive)
            return ReviewRecommendation.Warn;

        return ReviewRecommendation.Publish;
    }

    private ReviewSummaryDto? BuildSummary(string text)
    {
        var source = _masker.ContainsProfanity(text)
            ? StripAsterisks(_masker.Mask(text) ?? text)
            : text;

        var sentences = SentenceSplitRegex
            .Split(source)
            .Select(s => s.Trim())
            .Where(IsNonEmpty)
            .ToList();

        var summary = sentences.Count switch
        {
            0 => null,
            1 => Truncate(sentences[0], 140),
            _ => Truncate(sentences[0], 120) + " " + Truncate(sentences[1], 80)
        };

        return string.IsNullOrWhiteSpace(summary) ? null : new ReviewSummaryDto(summary, [], [], []);
    }

    private static double SentencePolarity(string sentence)
    {
        var tokens = TokenList(sentence);
        var score = 0.0;
        foreach (var token in tokens)
        {
            foreach (var (word, weight) in SentimentWords)
            {
                if (string.Equals(token, word, StringComparison.OrdinalIgnoreCase))
                {
                    score += weight;
                    break;
                }
            }
        }

        return score;
    }

    private static List<string> TokenList(string text)
    {
        var list = new List<string>();
        foreach (Match match in TokenRegex.Matches(text))
            list.Add(match.Value.ToLowerInvariant());
        return list;
    }

    private static bool IsNonEmpty(string value) => !string.IsNullOrWhiteSpace(value);

    private static string Truncate(string value, int max)
    {
        if (value.Length <= max)
            return value;

        var cut = value[..max];
        var space = cut.LastIndexOf(' ');
        return space > max / 2 ? cut[..space].TrimEnd(',', '.', ' ') : cut.TrimEnd(',', '.', ' ');
    }

    private static string StripAsterisks(string value)
        => Regex.Replace(value, @"\*+", " ").Trim();

    private static void AddDistinct(List<string> list, string value)
    {
        if (!list.Contains(value, StringComparer.OrdinalIgnoreCase))
            list.Add(value);
    }
}
