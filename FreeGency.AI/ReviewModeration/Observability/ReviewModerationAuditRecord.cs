using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// An immutable audit record for a single review moderation request. Only safe
/// data is stored: the review is represented by its SHA-256 hash, length, and
/// rating; entity values are never included, only counts and category names. This
/// keeps every request traceable (timestamp, request id, correlation id, prompt
/// version, model version, processing time, action, risk score) without exposing
/// the raw review text, emails, phones, cards, wallets, or keys.
/// </summary>
public sealed record ReviewModerationAuditRecord(
    Guid RequestId,
    Guid CorrelationId,
    Guid? ReviewId,
    Guid? ProjectId,
    Guid? ReviewerId,
    Guid? ReviewedUserId,
    string ReviewHash,
    int ReviewLength,
    int? Rating,
    string? Language,
    double RiskScore,
    ReviewAction Action,
    ReviewSentiment Sentiment,
    double Confidence,
    IReadOnlyList<ReviewSecurityCategory> SecurityCategories,
    int EntityCount,
    int CategoryCount,
    bool UsedCache,
    TimeSpan ProcessingTime,
    string PromptVersion,
    string? ModelName,
    DateTimeOffset Timestamp);
