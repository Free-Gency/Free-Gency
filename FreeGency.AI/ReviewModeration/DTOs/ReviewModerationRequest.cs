namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// A request to moderate a marketplace review. Only <see cref="ReviewText"/> is
/// required; the remaining fields add surrounding context so the model can make
/// a context-aware judgment.
/// </summary>
public sealed record ReviewModerationRequest
{
    /// <summary>Gets the identifier of the review being moderated.</summary>
    public Guid? ReviewId { get; init; }

    /// <summary>Gets the identifier of the project the review belongs to.</summary>
    public Guid? ProjectId { get; init; }

    /// <summary>Gets the identifier of the user who wrote the review.</summary>
    public Guid? ReviewerId { get; init; }

    /// <summary>Gets the identifier of the user being reviewed.</summary>
    public Guid? ReviewedUserId { get; init; }

    /// <summary>Gets the raw review text. Required and non-empty after normalization.</summary>
    public string ReviewText { get; init; } = string.Empty;

    /// <summary>Gets the optional star rating (1 to 5).</summary>
    public int? Rating { get; init; }

    /// <summary>Gets the optional language hint (for example "en" or "ar").</summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the reviewer's earlier reviews for the same project, when available,
    /// so the model can detect repeated/spam patterns. Optional; when present,
    /// only the rating and text are surfaced to the model.
    /// </summary>
    public IReadOnlyList<ReviewModerationRequest>? PreviousReviews { get; init; }

    /// <summary>Gets the moment the review was written, when known.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets optional key/value context (roles, job type, and so on).</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
