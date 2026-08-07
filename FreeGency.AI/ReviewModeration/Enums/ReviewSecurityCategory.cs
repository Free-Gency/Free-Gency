namespace FreeGency.AI.ReviewModeration.Enums;

/// <summary>
/// The typed security categories a review can be classified into. A review may
/// belong to one or more categories. These are the stable, contract-level names
/// produced by both the AI classification and the deterministic security layer.
/// </summary>
public enum ReviewSecurityCategory
{
    /// <summary>Swearing, offensive words, insults, or vulgar language.</summary>
    Profanity = 0,

    /// <summary>Personal attacks, abuse, threats, intimidation, or bullying.</summary>
    Harassment = 1,

    /// <summary>Hate toward race, religion, nationality, gender, disability, or ethnicity.</summary>
    HateSpeech = 2,

    /// <summary>Death threats, physical harm, weapon references, or violence encouragement.</summary>
    Violence = 3,

    /// <summary>Investment, crypto, gift card, money transfer, employment, or advance payment scams.</summary>
    Scam = 4,

    /// <summary>Fake reviews, review manipulation, paid reviews, or artificial reputation boosting.</summary>
    Fraud = 5,

    /// <summary>Repeated, copy-paste, duplicate, emoji, or character spam.</summary>
    Spam = 6,

    /// <summary>Promotion, referral codes, coupons, or external contact channels (phones, emails, URLs).</summary>
    Advertisement = 7,

    /// <summary>Credit cards, IBANs, passwords, OTPs, wallets, keys, passports, or national IDs.</summary>
    SensitiveInformation = 8,

    /// <summary>Attempts to override instructions or reveal system prompts.</summary>
    PromptInjection = 9,

    /// <summary>Attempts to extract LLM internals: system prompts, hidden
    /// instructions, chain-of-thought reasoning, configuration, keys, or secrets.</summary>
    LlmAbuse = 10,

    /// <summary>Personal attacks, bullying, harassment, violence, threats,
    /// extremism, sexism, racism, or religious/political hate.</summary>
    Toxicity = 11
}
