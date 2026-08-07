using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeGency.AI.ReviewModeration.Caching;

/// <summary>
/// Builds deterministic, versioned cache keys for review moderation results.
/// The key is a SHA-256 hash of the canonical review text, rating, language,
/// prompt version, model version, and moderation version, so the key changes
/// automatically whenever the prompt text, the model, or the business rules
/// change. Stateless and thread-safe.
/// </summary>
public sealed partial class ReviewModerationCacheKeyBuilder
{
    /// <summary>The namespace prefix for every review moderation cache key.</summary>
    public const string CacheKeyPrefix = "ai:reviewmoderation:";

    private const char HashSeparator = '\u001F';

    /// <summary>
    /// Builds a review moderation cache key. <paramref name="text"/> is normalized
    /// before hashing; <paramref name="promptVersion"/>, <paramref name="modelVersion"/>,
    /// and <paramref name="moderationVersion"/> are included verbatim so any change
    /// invalidates previously cached entries.
    /// </summary>
    public string BuildKey(
        string? text,
        int? rating,
        string? language,
        string promptVersion,
        string modelVersion,
        string moderationVersion)
    {
        var canonical = string.Join(
            HashSeparator.ToString(),
            Normalize(text),
            rating?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            (language ?? string.Empty).Trim().ToLowerInvariant(),
            promptVersion,
            modelVersion,
            moderationVersion);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return CacheKeyPrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Normalizes review text for hashing: trims whitespace, applies Unicode
    /// normalization form KC, normalizes line endings, collapses runs of
    /// whitespace to a single space, and removes invisible characters
    /// (zero-width spaces, bidi controls, word joiners, and BOMs).
    /// </summary>
    public string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormKC);
        normalized = normalized.Replace("\r\n", "\n").Replace('\r', '\n');
        normalized = InvisibleCharactersRegex().Replace(normalized, string.Empty);
        normalized = WhitespaceRegex().Replace(normalized, " ");
        return normalized.Trim();
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[\u200B-\u200F\u202A-\u202E\u2060-\u2064\uFEFF]", RegexOptions.CultureInvariant)]
    private static partial Regex InvisibleCharactersRegex();
}
