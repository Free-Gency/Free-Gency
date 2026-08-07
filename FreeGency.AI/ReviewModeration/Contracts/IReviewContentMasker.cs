using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// Deterministically masks profanity and sensitive values (phones, emails,
/// credit cards, wallets, passwords, keys, URLs) inside any text. Stateless and
/// thread-safe; reused by review moderation and reusable for comments, chat,
/// profiles, portfolio, and project descriptions.
/// </summary>
public interface IReviewContentMasker
{
    /// <summary>
    /// Detects sensitive values (emails, phones, cards, wallets, keys, passwords,
    /// external URLs) in <paramref name="text"/>. Returns an empty list when none
    /// are found.
    /// </summary>
    IReadOnlyList<ReviewSensitiveMatch> FindSensitive(string text);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="text"/> contains profanity in any
    /// supported form (Arabic, English, Franco Arabic, masked, or leet spellings).
    /// </summary>
    bool ContainsProfanity(string text);

    /// <summary>
    /// Returns <paramref name="text"/> with every profanity and sensitive value
    /// replaced by asterisks, or <c>null</c> when nothing needed masking.
    /// </summary>
    string? Mask(string text);
}
