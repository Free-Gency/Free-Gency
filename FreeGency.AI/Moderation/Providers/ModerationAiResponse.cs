using FreeGency.AI.Moderation.Models;

namespace FreeGency.AI.Moderation.Providers;

/// <summary>
/// The raw JSON shape the moderation model returns. All fields are optional and
/// are mapped defensively by <see cref="ModerationJsonParser"/>.
/// </summary>
public sealed class ModerationAiResponse
{
    public bool IsSafe { get; set; }
    public double? RiskScore { get; set; }
    public double? Confidence { get; set; }
    public string? RiskLevel { get; set; }
    public string? Action { get; set; }
    public string? Reason { get; set; }
    public List<ModerationCategoryScore> Categories { get; set; } = [];
    public List<string> FlaggedKeywords { get; set; } = [];
}
