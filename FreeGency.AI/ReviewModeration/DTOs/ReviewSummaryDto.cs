namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// A generated summary of a review with its key points and topics.
/// </summary>
/// <param name="ShortSummary">A one-sentence summary of the review.</param>
/// <param name="PositivePoints">The positive points mentioned by the reviewer.</param>
/// <param name="NegativePoints">The negative points mentioned by the reviewer.</param>
/// <param name="KeyTopics">The main topics covered by the review.</param>
public sealed record ReviewSummaryDto(
    string ShortSummary,
    IReadOnlyList<string> PositivePoints,
    IReadOnlyList<string> NegativePoints,
    IReadOnlyList<string> KeyTopics);
