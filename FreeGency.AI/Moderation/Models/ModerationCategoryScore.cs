namespace FreeGency.AI.Moderation.Models;

/// <summary>
/// A single violation category reported by the moderation engine, with the
/// model's confidence score (0..1) for that category.
/// </summary>
public sealed record ModerationCategoryScore(string Category, double Score)
{
    /// <inheritdoc />
    public override string ToString() => $"{Category}:{Score:0.00}";
}
