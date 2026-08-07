namespace FreeGency.AI.ChatModeration.Enums;

/// <summary>
/// The risk categories a moderated message can be flagged under.
/// </summary>
public enum ModerationCategory
{
    /// <summary>Unsolicited or repetitive bulk messaging.</summary>
    Spam,

    /// <summary>Excessive message frequency or volume.</summary>
    Flood,

    /// <summary>Promotional or unsolicited advertising.</summary>
    Advertisement,

    /// <summary>Fraudulent schemes designed to deceive.</summary>
    Scam,

    /// <summary>Deception intended for financial or personal gain.</summary>
    Fraud,

    /// <summary>Attempts to trick users into revealing credentials or data.</summary>
    Phishing,

    /// <summary>Attempts to move the conversation off the platform.</summary>
    ExternalContact,

    /// <summary>Leakage of personal or sensitive information.</summary>
    SensitiveInformation,

    /// <summary>Profane or vulgar language.</summary>
    Profanity,

    /// <summary>Language that is toxic or hostile.</summary>
    ToxicLanguage,

    /// <summary>Insulting or demeaning remarks.</summary>
    Insult,

    /// <summary>Harassing or bullying behavior.</summary>
    Harassment,

    /// <summary>Statements threatening harm.</summary>
    Threat,

    /// <summary>Promotion or glorification of violence.</summary>
    Violence,

    /// <summary>Speech that attacks or demeans a group.</summary>
    HateSpeech,

    /// <summary>Discrimination based on race or ethnicity.</summary>
    Racism,

    /// <summary>Abuse targeting a religion or its followers.</summary>
    ReligiousAbuse,

    /// <summary>Abuse targeting a nationality or its people.</summary>
    NationalityAbuse,

    /// <summary>Abuse targeting a gender or sexual orientation.</summary>
    GenderAbuse,

    /// <summary>Sexual or explicit content.</summary>
    SexualContent,

    /// <summary>Content promoting or depicting self-harm.</summary>
    SelfHarm,

    /// <summary>Attempts to override the moderation instructions.</summary>
    PromptInjection,

    /// <summary>Attempts to remove model or platform safeguards.</summary>
    Jailbreak,

    /// <summary>Content promoting or distributing malware.</summary>
    Malware,

    /// <summary>Content promoting illegal activity.</summary>
    IllegalActivity,

    /// <summary>Content related to drugs or substance abuse.</summary>
    DrugRelated,

    /// <summary>Content related to weapons.</summary>
    Weapons,

    /// <summary>Attempts to extort through threats or secrets.</summary>
    Blackmail,

    /// <summary>Attempts to steal or impersonate identities.</summary>
    IdentityTheft,

    /// <summary>Pretending to be another person or entity.</summary>
    Impersonation,

    /// <summary>Manipulating users into revealing information.</summary>
    SocialEngineering,

    /// <summary>Fraudulent schemes involving cryptocurrencies.</summary>
    CryptoScam,

    /// <summary>Fake support agents offering fraudulent help.</summary>
    FakeSupport
}
