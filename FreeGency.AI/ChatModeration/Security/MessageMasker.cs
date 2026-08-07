using System.Text.RegularExpressions;
using FreeGency.AI.ChatModeration.Constants;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Finds profanity keywords in a normalized message and masks sensitive entities
/// and profanity in the original message. Thread-safe and stateless.
/// </summary>
public sealed class MessageMasker
{
    /// <summary>
    /// Returns the profanity keywords and abusive emojis found in a normalized message.
    /// </summary>
    public IReadOnlyList<string> FindProfanity(string normalizedMessage)
    {
        if (string.IsNullOrEmpty(normalizedMessage))
            return [];

        var found = new List<string>();

        foreach (var keyword in ModerationLexicon.Profanity)
        {
            if (normalizedMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                found.Add(keyword);
        }

        foreach (var emoji in ModerationLexicon.AbusiveEmojis)
        {
            if (normalizedMessage.Contains(emoji, StringComparison.Ordinal))
                found.Add(emoji);
        }

        return found;
    }

    /// <summary>
    /// Masks sensitive entities and profanity in the original message. Entities are
    /// masked by index; profanity is replaced with "---".
    /// </summary>
    public string Mask(string message, IReadOnlyList<DetectedEntityDto> entities, IReadOnlyList<string> profanityKeywords)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var masked = message;

        foreach (var entity in entities.OrderByDescending(e => e.StartIndex))
        {
            var length = entity.EndIndex - entity.StartIndex;
            if (length <= 0 || entity.StartIndex < 0 || entity.EndIndex > masked.Length)
                continue;

            masked = masked.Remove(entity.StartIndex, length)
                           .Insert(entity.StartIndex, MaskEntityValue(entity));
        }

        if (profanityKeywords.Count > 0)
        {
            var pattern = string.Join("|", profanityKeywords.Select(Regex.Escape));
            masked = Regex.Replace(masked, pattern, "---", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        return masked;
    }

    private static string MaskEntityValue(DetectedEntityDto entity)
    {
        var value = entity.Value;

        return entity.Type switch
        {
            DetectedEntityType.Phone => MaskKeep(value, 3, 2),
            DetectedEntityType.Email => MaskEmail(value),
            DetectedEntityType.CreditCard => "****" + Last(value, 4),
            DetectedEntityType.CryptoWallet => MaskKeep(value, 6, 4),
            DetectedEntityType.IBAN => MaskKeep(value, 4, 4),
            DetectedEntityType.Passport => MaskKeep(value, 2, 2),
            DetectedEntityType.NationalId => MaskKeep(value, 3, 3),
            DetectedEntityType.Password => "********",
            DetectedEntityType.URL => "***",
            _ => "***"
        };
    }

    private static string MaskKeep(string value, int first, int last)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= first + last)
            return new string('*', value.Length);

        return value[..first] + new string('*', value.Length - first - last) + value[^last..];
    }

    private static string MaskEmail(string value)
    {
        var at = value.IndexOf('@');
        if (at <= 0)
            return "***";

        var local = value[..at];
        var domain = value[at..];

        if (local.Length <= 2)
            return "****" + domain;

        return local[..2] + "****" + domain;
    }

    private static string Last(string value, int count)
    {
        return value.Length <= count ? value : value[^count..];
    }
}
