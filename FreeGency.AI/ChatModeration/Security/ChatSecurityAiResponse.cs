namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// The raw JSON shape the moderation model returns. All fields are optional and
/// are mapped defensively by <see cref="ChatSecurityJsonParser"/>.
/// </summary>
public sealed class ChatSecurityAiResponse
{
    public bool IsSafe { get; set; }
    public double? RiskScore { get; set; }
    public double? Confidence { get; set; }
    public string? RiskLevel { get; set; }
    public string? Action { get; set; }
    public string? Reason { get; set; }
    public List<string> DetectedLanguages { get; set; } = [];
    public List<ChatSecurityCategoryDto> Categories { get; set; } = [];
    public List<string> MatchedKeywords { get; set; } = [];
    public List<ChatSecurityEntityDto> DetectedEntities { get; set; } = [];
    public string? MaskedMessage { get; set; }
}

/// <summary>
/// A single category reported by the model. The category name maps to
/// <see cref="Enums.ModerationCategory"/> and the score is 0..1.
/// </summary>
public sealed class ChatSecurityCategoryDto
{
    public string? Category { get; set; }
    public double? Score { get; set; }
}

/// <summary>
/// A single entity reported by the model. The type maps to
/// <see cref="Enums.DetectedEntityType"/>.
/// </summary>
public sealed class ChatSecurityEntityDto
{
    public string? Type { get; set; }
    public string? Value { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public double? Confidence { get; set; }
}
