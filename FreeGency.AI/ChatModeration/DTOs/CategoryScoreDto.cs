using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.DTOs;

/// <summary>
/// The risk score assigned to a single moderation category.
/// </summary>
/// <param name="Category">The moderation category.</param>
/// <param name="Score">The category risk score on a 0..1 scale.</param>
public sealed record CategoryScoreDto(
    ModerationCategory Category,
    double Score);
