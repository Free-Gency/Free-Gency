namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class ProjectRankingRequest
{
    public required string ProjectId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<ProposalCandidate> Candidates { get; init; }
    public required RankingCriteria Criteria { get; init; }
    public int TopK { get; init; } = 5;
}

public sealed class RankingCriteria
{
    public IReadOnlyList<string>? RequiredSkills { get; init; }
    public IReadOnlyList<string>? PreferredSkills { get; init; }
    public decimal? BudgetMin { get; init; }
    public decimal? BudgetMax { get; init; }
    public string? ExperienceLevel { get; init; }
    public int? MinCompletedProjects { get; init; }
    public double? MinRating { get; init; }
    public string? Timeline { get; init; }
}
