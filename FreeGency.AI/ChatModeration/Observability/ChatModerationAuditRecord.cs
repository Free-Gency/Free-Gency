using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// An immutable audit record for a single moderation request. Only safe data is
/// stored: the message is represented by its SHA-256 hash and lengths; entity
/// values are never included, only counts and category names. This keeps every
/// request traceable without exposing passwords, credit cards, OTPs, private
/// keys, seed phrases, national IDs, passport numbers, raw phones, or raw
/// emails.
/// </summary>
public sealed record ChatModerationAuditRecord(
    Guid RequestId,
    Guid CorrelationId,
    string? ConversationId,
    string? ProjectId,
    string? SenderId,
    string? ReceiverId,
    string MessageHash,
    int OriginalMessageLength,
    int? MaskedMessageLength,
    ContentLanguage Language,
    double RiskScore,
    double Confidence,
    RiskLevel RiskLevel,
    ModerationAction Action,
    IReadOnlyList<string> DetectedCategories,
    int DetectedEntitiesCount,
    IReadOnlyList<string> MatchedKeywords,
    bool UsedCache,
    int RetryCount,
    TimeSpan ProcessingTime,
    string PromptVersion,
    string? ModelName,
    DateTimeOffset Timestamp);
