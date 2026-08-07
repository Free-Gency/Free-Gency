namespace FreeGency.AI.ReviewModeration.DTOs;

/// <summary>
/// The risk score assigned to a single policy violation category.
/// </summary>
/// <param name="Category">The violation category name.</param>
/// <param name="Score">The category risk score on a 0..1 scale.</param>
public sealed record ReviewCategoryDto(string Category, double Score);
