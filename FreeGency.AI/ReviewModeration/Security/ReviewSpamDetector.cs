using System.Text.RegularExpressions;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Enums;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Security;

/// <summary>
/// Deterministic spam detector. Flags repeated, copy-paste, emoji, character,
/// random, repetitive-word, meaningless, and very short reviews. When the
/// reviewer's previous reviews are supplied, the text is compared against them
/// to catch duplicate reviews. Stateless and thread-safe.
/// </summary>
public sealed class ReviewSpamDetector : IReviewSpamDetector
{
    private const double DuplicateScore = 65;
    private const double EmojiScore = 55;
    private const double CharacterScore = 45;
    private const double WordScore = 40;
    private const double DiversityScore = 40;
    private const double VeryShortScore = 15;
    private const double DuplicateThreshold = 0.80;

    private static readonly Regex RepeatedCharRegex = new(@"(.)\1{7,}", RegexOptions.Compiled);
    private static readonly Regex WordRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public ReviewSpamResult Detect(string text, IReadOnlyList<ReviewModerationRequest>? previousReviews)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new ReviewSpamResult([], 0);

        var signals = new List<ReviewSpamSignal>();
        var score = 0d;

        if (text.Trim().Length < 8)
        {
            signals.Add(ReviewSpamSignal.VeryShort);
            score = Math.Max(score, VeryShortScore);
        }

        if (CountEmojis(text) >= 6)
        {
            signals.Add(ReviewSpamSignal.EmojiSpam);
            score = Math.Max(score, EmojiScore);
        }

        if (RepeatedCharRegex.IsMatch(text))
        {
            signals.Add(ReviewSpamSignal.CharacterSpam);
            score = Math.Max(score, CharacterScore);
        }

        if (HasRepeatedWords(text))
        {
            signals.Add(ReviewSpamSignal.RepeatedWords);
            score = Math.Max(score, WordScore);
        }

        if (IsLowDiversity(text))
        {
            signals.Add(ReviewSpamSignal.RandomText);
            signals.Add(ReviewSpamSignal.MeaninglessText);
            score = Math.Max(score, DiversityScore);
        }

        if (previousReviews is { Count: > 0 })
        {
            var similarity = MaxSimilarity(text, previousReviews);
            if (similarity >= DuplicateThreshold)
            {
                signals.Add(similarity >= 0.98 ? ReviewSpamSignal.DuplicateReview : ReviewSpamSignal.CopyPaste);
                score = Math.Max(score, DuplicateScore);
            }
        }

        return new ReviewSpamResult(signals.Distinct().ToList(), score);
    }

    private static bool HasRepeatedWords(string text)
    {
        var tokens = TokenList(text);
        if (tokens.Count < 12)
            return false;

        var counts = tokens
            .GroupBy(t => t, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var (_, count) in counts)
        {
            if (count >= 5 && (double)count / tokens.Count >= 0.40)
                return true;
        }

        return false;
    }

    private static bool IsLowDiversity(string text)
    {
        var total = 0;
        var seen = new HashSet<char>();
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
                continue;

            total++;
            seen.Add(char.ToLowerInvariant(ch));
        }

        return total > 20 && (double)seen.Count / total < 0.15;
    }

    private static double MaxSimilarity(string text, IReadOnlyList<ReviewModerationRequest> previousReviews)
    {
        var current = TokenSet(text);
        if (current.Count == 0)
            return 0;

        var best = 0d;
        foreach (var previous in previousReviews)
        {
            if (string.IsNullOrWhiteSpace(previous.ReviewText))
                continue;

            var other = TokenSet(previous.ReviewText);
            if (other.Count == 0)
                continue;

            best = Math.Max(best, Jaccard(current, other));
            if (best >= 1.0)
                break;
        }

        return best;
    }

    private static HashSet<string> TokenSet(string text)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in WordRegex.Matches(text))
            set.Add(match.Value.ToLowerInvariant());

        return set;
    }

    private static List<string> TokenList(string text)
    {
        var list = new List<string>();
        foreach (Match match in WordRegex.Matches(text))
            list.Add(match.Value.ToLowerInvariant());

        return list;
    }

    private static double Jaccard(HashSet<string> a, HashSet<string> b)
    {
        var intersection = 0;
        foreach (var word in a)
        {
            if (b.Contains(word))
                intersection++;
        }

        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static int CountEmojis(string text)
    {
        var count = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                var codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                if (IsEmojiCodePoint(codePoint))
                    count++;
                i++;
            }
            else if (IsEmojiCodePoint(text[i]))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsEmojiCodePoint(int codePoint)
        => (codePoint is >= 0x1F000 and <= 0x1FAFF)
        || (codePoint is >= 0x1F1E6 and <= 0x1F1FF)
        || (codePoint is >= 0x2600 and <= 0x27BF)
        || (codePoint is >= 0x2B00 and <= 0x2BFF)
        || codePoint == 0xFE0F;
}
