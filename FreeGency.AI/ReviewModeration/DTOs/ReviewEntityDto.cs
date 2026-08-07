namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// A single entity detected inside a review, such as a person, company,
/// location, email address, or phone number.
/// </summary>
/// <param name="Type">The kind of entity that was detected.</param>
/// <param name="Value">The detected value as written in the review.</param>
/// <param name="Confidence">The detection confidence on a 0..1 scale.</param>
public sealed record ReviewEntityDto(string Type, string Value, double Confidence);
