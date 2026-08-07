namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// A suggested improvement for a review so the author can make it more useful
/// and compliant before publication.
/// </summary>
/// <param name="Type">The kind of suggestion (for example "Softening", "Evidence", "Specifics", or "Formatting").</param>
/// <param name="Message">The human-readable suggestion.</param>
/// <param name="Severity">The suggestion importance ("Low", "Medium", or "High").</param>
public sealed record ReviewSuggestionDto(string Type, string Message, string Severity);
