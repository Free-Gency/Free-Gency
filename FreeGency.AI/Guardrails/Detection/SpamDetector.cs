using System.Text.RegularExpressions;

namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic spam detector: very short, emoji, repeated-character, repeated
/// word, low-diversity random text, copy-paste, and duplicate text (compared
/// against the prior texts in the detector context). Produces a 0..100 risk
/// contribution alongside the triggered signals.
/// </summary>
public sealed class SpamDetector : IGuardrailDetector
{
    private const double DuplicateScore = 65;
    private const double EmojiScore = 55;
    private const double CharacterScore = 45;
    private const double WordScore = 40;
    private const double DiversityScore = 40;
    private const double VeryShortScore = 15;
    private const double DuplicateThreshold = 0.80;

    private static readonly Regex RepeatedCharRegex = new(@"(.)\1{7,}", RegexOptions.Compiled);

    /// <inheritdoc />
    public string Name => "Spam";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableSpam;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        if (text.Trim().Length < 8)
            builder.AddSpamSignal(SpamSignalType.VeryShort, VeryShortScore);

        if (GuardrailText.CountEmojis(text) >= 6)
            builder.AddSpamSignal(SpamSignalType.EmojiSpam, EmojiScore);

        if (RepeatedCharRegex.IsMatch(text))
            builder.AddSpamSignal(SpamSignalType.CharacterSpam, CharacterScore);

        if (HasRepeatedWords(text))
            builder.AddSpamSignal(SpamSignalType.RepeatedWords, WordScore);

        if (IsLowDiversity(text))
        {
            builder.AddSpamSignal(SpamSignalType.RandomText, DiversityScore);
            builder.AddSpamSignal(SpamSignalType.MeaninglessText, DiversityScore);
        }

        if (context.PriorTexts is { Count: > 0 })
        {
            var similarity = MaxSimilarity(text, context.PriorTexts);
            if (similarity >= DuplicateThreshold)
            {
                builder.AddSpamSignal(
                    similarity >= 0.98 ? SpamSignalType.RepeatedReview : SpamSignalType.CopyPaste,
                    DuplicateScore);
            }
        }
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

    private static double MaxSimilarity(string text, IReadOnlyList<string> priorTexts)
    {
        var current = TokenSet(text);
        if (current.Count == 0)
            return 0;

        var best = 0d;
        foreach (var prior in priorTexts)
        {
            if (string.IsNullOrWhiteSpace(prior))
                continue;

            var other = TokenSet(prior);
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
        foreach (var token in GuardrailText.Tokenize(text))
            set.Add(token);

        return set;
    }

    private static List<string> TokenList(string text) => GuardrailText.Tokenize(text);

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
}
