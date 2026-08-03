namespace FreeGency.Application.Features.TeamSuggestions.DTOs;

public sealed class TeamSuggestionResponseDto
{
    public string DeveloperId { get; init; } = string.Empty;
    public string? AiSummary { get; init; }
    public List<RankedTeamSuggestionDto> RankedTeams { get; init; } = [];
    public TeamSuggestionMetadataDto? Metadata { get; init; }
}

public sealed class RankedTeamSuggestionDto
{
    public string TeamId { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public string? TeamLogoUrl { get; init; }
    public string? JobId { get; init; }
    public string? JobTitle { get; init; }
    public double Score { get; init; }
    public double Confidence { get; init; }
    public string? Summary { get; init; }
    public string? Reason { get; init; }
    public List<string> Strengths { get; init; } = [];
    public List<string> Weaknesses { get; init; } = [];
    public List<string> JobSkills { get; init; } = [];
    public int MemberCount { get; init; }
    public double AverageRating { get; init; }
    public int RatingCount { get; init; }
    /// <summary>True when the developer already sent a join request for this team's job.</summary>
    public bool HasApplied { get; init; }
}

public sealed class TeamSuggestionMetadataDto
{
    public int TotalCandidatesEvaluated { get; init; }
    public int ReturnedCount { get; init; }
    public double ProcessingTimeMs { get; init; }
    public bool UsedFallbackScoring { get; init; }
    public List<string> Warnings { get; init; } = [];
}
