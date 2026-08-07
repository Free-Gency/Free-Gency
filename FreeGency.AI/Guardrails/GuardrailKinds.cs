namespace FreeGency.AI.Guardrails;

/// <summary>
/// The kinds of sensitive values the guardrail engine can detect. These are the
/// stable, contract-level names used by the review, chat, and profile modules.
/// </summary>
public enum SensitiveDataKind
{
    Email = 0,
    PhoneNumber = 1,
    CreditCard = 2,
    Iban = 3,
    BankAccount = 4,
    WalletAddress = 5,
    PrivateKey = 6,
    Password = 7,
    Otp = 8,
    ApiKey = 9,
    AccessToken = 10,
    Jwt = 11,
    PassportNumber = 12,
    NationalId = 13,
    DriverLicense = 14
}

/// <summary>
/// The kinds of prompt-injection attempts the guardrail engine can detect.
/// </summary>
public enum PromptInjectionKind
{
    IgnorePreviousInstructions = 0,
    ForgetRules = 1,
    DeveloperMode = 2,
    SystemPromptReveal = 3,
    Jailbreak = 4,
    ActAs = 5,
    Pretend = 6,
    OverrideInstructions = 7,
    ExecuteCode = 8,
    SystemOverride = 9,
    RoleSwitching = 10,
    InstructionHijacking = 11,
    HiddenPrompt = 12,
    NestedPrompt = 13,
    PromptChaining = 14,
    RecursivePrompting = 15,
    IndirectPromptInjection = 16
}

/// <summary>
/// The toxicity categories the guardrail engine can flag. Subcategories of the
/// broader harassment/violence families so moderators can triage precisely.
/// </summary>
public enum ToxicityCategory
{
    PersonalAttack = 0,
    Bullying = 1,
    Harassment = 2,
    Violence = 3,
    Threat = 4,
    Extremism = 5,
    Sexism = 6,
    Racism = 7,
    ReligiousHate = 8,
    PoliticalHate = 9
}

/// <summary>
/// The scam categories the guardrail engine can flag.
/// </summary>
public enum ScamCategory
{
    InvestmentScam = 0,
    CryptoScam = 1,
    AdvancePaymentScam = 2,
    GiftCardScam = 3,
    EmploymentScam = 4,
    RefundScam = 5
}

/// <summary>
/// The deterministic spam signals the guardrail engine can flag. Mirrors the
/// review module's spam signals so the same names appear across modules.
/// </summary>
public enum SpamSignalType
{
    VeryShort = 0,
    EmojiSpam = 1,
    CharacterSpam = 2,
    RepeatedWords = 3,
    RandomText = 4,
    MeaninglessText = 5,
    CopyPaste = 6,
    RepeatedReview = 7
}
