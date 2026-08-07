namespace FreeGency.AI.ChatModeration.Enums;

/// <summary>
/// The kinds of sensitive or personally identifiable entities that can be detected.
/// </summary>
public enum DetectedEntityType
{
    /// <summary>A telephone number.</summary>
    Phone,

    /// <summary>An email address.</summary>
    Email,

    /// <summary>A credit card number.</summary>
    CreditCard,

    /// <summary>An International Bank Account Number.</summary>
    IBAN,

    /// <summary>A national identity document number.</summary>
    NationalId,

    /// <summary>A passport number.</summary>
    Passport,

    /// <summary>A password or credential.</summary>
    Password,

    /// <summary>A cryptocurrency wallet address.</summary>
    CryptoWallet,

    /// <summary>A web URL.</summary>
    URL,

    /// <summary>A Telegram handle.</summary>
    Telegram,

    /// <summary>A WhatsApp number or link.</summary>
    WhatsApp,

    /// <summary>A Discord handle or invite.</summary>
    Discord,

    /// <summary>A Facebook account or page.</summary>
    Facebook,

    /// <summary>An Instagram account.</summary>
    Instagram,

    /// <summary>A LinkedIn profile.</summary>
    LinkedIn,

    /// <summary>An X (Twitter) account.</summary>
    Twitter,

    /// <summary>A GitHub account.</summary>
    GitHub
}
