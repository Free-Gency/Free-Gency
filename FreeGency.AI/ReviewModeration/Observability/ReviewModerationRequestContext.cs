using System.Security.Cryptography;
using System.Text;
using FreeGency.AI.Monitoring;
using FreeGency.AI.ReviewModeration.DTOs;

namespace FreeGency.AI.ReviewModeration.Observability;

/// <summary>
/// Immutable per-request context used to stamp every review moderation log line
/// and audit record. Contains only non-sensitive data: identifiers, the SHA-256
/// hash of the review text, the review length, the language hint, and the rating.
/// The raw review text is never stored.
/// </summary>
public sealed record ReviewModerationRequestContext(
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
    string PromptVersion,
    DateTimeOffset Timestamp) : IModerationRequestContext
{
    /// <summary>
    /// Builds a fresh request context from a request, generating a new request id
    /// and correlation id and hashing the review text.
    /// </summary>
    public static ReviewModerationRequestContext FromRequest(ReviewModerationRequest request, string promptVersion)
    {
        var text = request.ReviewText ?? string.Empty;

        return new ReviewModerationRequestContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            request.ReviewId,
            request.ProjectId,
            request.ReviewerId,
            request.ReviewedUserId,
            ComputeHash(text),
            text.Length,
            request.Rating,
            request.Language,
            promptVersion,
            DateTimeOffset.UtcNow);
    }

    /// <summary>Computes the lowercase hex SHA-256 hash of <paramref name="text"/>. Empty input yields an empty string.</summary>
    public static string ComputeHash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }
}
