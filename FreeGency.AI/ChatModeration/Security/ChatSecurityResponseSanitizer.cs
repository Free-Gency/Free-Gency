using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Produces a copy of a moderation response that is safe to persist. Raw values
/// of sensitive or personally identifiable entities (passwords, card numbers,
/// national IDs, passports, IBANs, wallets, phones, emails, URLs) are replaced
/// with masked forms so secrets never reach the cache. Thread-safe and stateless.
/// </summary>
public sealed class ChatSecurityResponseSanitizer
{
    /// <summary>
    /// Returns a copy of <paramref name="response"/> whose entity values are
    /// masked. All other fields are preserved.
    /// </summary>
    public ChatModerationResponse Sanitize(ChatModerationResponse response)
    {
        if (response.DetectedEntities.Count == 0)
            return response;

        var entities = new List<DetectedEntityDto>(response.DetectedEntities.Count);
        foreach (var entity in response.DetectedEntities)
        {
            entities.Add(entity with { Value = MaskValue(entity) });
        }

        return response with { DetectedEntities = entities };
    }

    private static string MaskValue(DetectedEntityDto entity)
    {
        if (string.IsNullOrEmpty(entity.Value))
            return entity.Value;

        return entity.Type switch
        {
            DetectedEntityType.Password => "********",
            DetectedEntityType.CreditCard => "****" + Last(entity.Value, 4),
            DetectedEntityType.CryptoWallet => MaskKeep(entity.Value, 6, 4),
            DetectedEntityType.IBAN => MaskKeep(entity.Value, 4, 4),
            DetectedEntityType.NationalId => MaskKeep(entity.Value, 3, 3),
            DetectedEntityType.Passport => MaskKeep(entity.Value, 2, 2),
            DetectedEntityType.Phone => MaskKeep(entity.Value, 3, 2),
            DetectedEntityType.Email => MaskEmail(entity.Value),
            DetectedEntityType.URL => "***",
            _ => entity.Value
        };
    }

    private static string MaskKeep(string value, int first, int last)
    {
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
