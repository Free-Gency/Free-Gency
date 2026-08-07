using System.Text.RegularExpressions;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Extracts sensitive or personally identifiable entities (phones, emails, URLs,
/// crypto wallets, credit cards, passwords, IBANs, passports, national IDs) from a
/// message using regex patterns. Thread-safe and stateless.
/// </summary>
public sealed class EntityDetector
{
    private static readonly (DetectedEntityType Type, Regex Pattern, double Confidence)[] Patterns =
    [
        (DetectedEntityType.Email,
            new Regex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.CultureInvariant), 0.99),
        (DetectedEntityType.URL,
            new Regex(@"https?://[^\s<>""']+", RegexOptions.CultureInvariant), 0.98),
        (DetectedEntityType.URL,
            new Regex(@"\bwww\.[^\s<>""']+", RegexOptions.CultureInvariant), 0.9),
        (DetectedEntityType.CryptoWallet,
            new Regex(@"\b(bc1|[13])[A-HJ-NP-Za-km-z1-9]{25,39}\b", RegexOptions.CultureInvariant), 0.95),
        (DetectedEntityType.CryptoWallet,
            new Regex(@"\b0x[a-fA-F0-9]{40}\b", RegexOptions.CultureInvariant), 0.95),
        (DetectedEntityType.CryptoWallet,
            new Regex(@"\bT[A-HJ-NP-Za-km-z1-9]{33}\b", RegexOptions.CultureInvariant), 0.9),
        (DetectedEntityType.IBAN,
            new Regex(@"\b[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}\b", RegexOptions.CultureInvariant), 0.92),
        (DetectedEntityType.NationalId,
            new Regex(@"(?<!\d)[0-9]{14}(?!\d)", RegexOptions.CultureInvariant), 0.9),
        (DetectedEntityType.Passport,
            new Regex(@"\b[A-Z]{1,2}[0-9]{7,9}\b", RegexOptions.CultureInvariant), 0.8),
        (DetectedEntityType.Phone,
            new Regex(@"\b01[0125][0-9]{8}\b", RegexOptions.CultureInvariant), 0.98),
        (DetectedEntityType.Phone,
            new Regex(@"(?<!\d)\+?[0-9]{10,13}(?!\d)", RegexOptions.CultureInvariant), 0.9),
        (DetectedEntityType.CreditCard,
            new Regex(@"(?<!\d)(?:[0-9][ -]?){13,19}(?!\d)", RegexOptions.CultureInvariant), 0.85),
        (DetectedEntityType.Password,
            new Regex(@"(?i)\b(password|pass|pw)\s*[:=]\s*\S+", RegexOptions.CultureInvariant), 0.85)
    ];

    public IReadOnlyList<DetectedEntityDto> Detect(string message)
    {
        if (string.IsNullOrEmpty(message))
            return [];

        var matches = new List<(DetectedEntityDto Entity, int Priority)>();

        for (var priority = 0; priority < Patterns.Length; priority++)
        {
            var (type, pattern, confidence) = Patterns[priority];

            foreach (Match match in pattern.Matches(message))
            {
                if (match.Length == 0)
                    continue;

                matches.Add((
                    new DetectedEntityDto(type, match.Value, match.Index, match.Index + match.Length, confidence),
                    priority));
            }
        }

        var ordered = matches
            .OrderBy(m => m.Entity.StartIndex)
            .ThenBy(m => m.Priority)
            .ToList();

        var result = new List<DetectedEntityDto>(ordered.Count);

        foreach (var (entity, _) in ordered)
        {
            if (result.Count > 0 && entity.StartIndex < result[^1].EndIndex)
                continue;

            result.Add(entity);
        }

        return result;
    }
}
