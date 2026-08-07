namespace FreeGency.AI.Moderation.Models;

/// <summary>
/// The version of the moderation prompt template in use. The current production
/// version is <c>1.0</c>. The prompt version is part of every moderation cache
/// key, so bumping it automatically invalidates all previously cached results.
/// </summary>
public sealed record PromptVersion(string Value)
{
    /// <summary>Gets the current production prompt version (<c>1.0</c>).</summary>
    public static PromptVersion Current { get; } = new("1.0");

    /// <inheritdoc />
    public override string ToString() => Value;
}
