using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Observability;

/// <summary>
/// Immutable per-request context used to stamp every log line with the audit
/// fields required by the audit rules: request id, correlation id, conversation
/// id, and UTC timestamp. Contains only non-sensitive data (hashes and lengths,
/// never the raw message).
/// </summary>
public sealed record ChatModerationRequestContext(
    Guid RequestId,
    Guid CorrelationId,
    string? ConversationId,
    string? ProjectId,
    string? SenderId,
    string? ReceiverId,
    string MessageHash,
    int OriginalMessageLength,
    ContentLanguage Language,
    string PromptVersion,
    DateTimeOffset Timestamp);
