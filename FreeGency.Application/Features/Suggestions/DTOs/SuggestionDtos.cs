namespace FreeGency.Application.Features.Suggestions.DTOs;

public sealed class TeamsForMeResponseDto
{
    public List<SuggestedTeamJobDto> Suggestions { get; init; } = [];
    public SuggestionMetadataDto Metadata { get; init; } = new();
}

public sealed class SuggestedTeamJobDto
{
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public decimal TeamAverageRating { get; init; }
    public int TeamRatingCount { get; init; }
    public Guid JobId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;
    public List<string> RequiredSkills { get; init; } = [];
    public float FinalScore { get; init; }
    public float VectorScore { get; init; }
    public SuggestionScoreBreakdownDto Breakdown { get; init; } = new();
}

public sealed class ProjectCandidatesResponseDto
{
    public Guid ProjectId { get; init; }
    public List<SuggestedCandidateDto> Candidates { get; init; } = [];
    public SuggestionMetadataDto Metadata { get; init; } = new();
}

public sealed class SuggestedCandidateDto
{
    public string CandidateType { get; init; } = string.Empty;
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? About { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public decimal AverageRating { get; init; }
    public int RatingCount { get; init; }
    public List<string> Skills { get; init; } = [];
    public List<string> Specialties { get; init; } = [];
    public List<string> Categories { get; init; } = [];
    public int PortfolioProjectCount { get; init; }
    public int CompletedProjectsCount { get; init; }
    public int? MemberCount { get; init; }
    public Guid? FeaturedPortfolioProjectId { get; init; }
    public string? FeaturedPortfolioTitle { get; init; }
    public float FinalScore { get; init; }
    public float VectorScore { get; init; }
    public SuggestionScoreBreakdownDto Breakdown { get; init; } = new();
}

public sealed class SuggestionScoreBreakdownDto
{
    public float Vector { get; init; }
    public float SkillOverlap { get; init; }
    public float SpecialtyOverlap { get; init; }
    public float Rating { get; init; }
    public float Portfolio { get; init; }
}

public sealed class SuggestionMetadataDto
{
    public int ReturnedCount { get; init; }
    public long ElapsedMs { get; init; }
    public List<string> Warnings { get; init; } = [];
}

public sealed class ReindexResultDto
{
    public int DevelopersIndexed { get; init; }
    public int TeamsIndexed { get; init; }
    public int TeamJobsIndexed { get; init; }
    public int ProjectsIndexed { get; init; }
    public long ElapsedMs { get; init; }
}
