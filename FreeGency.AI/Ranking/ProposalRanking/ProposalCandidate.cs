namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class ProposalCandidate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Headline { get; init; }
    public string? Bio { get; init; }
    public IReadOnlyList<CandidateSkill>? Skills { get; init; }
    public CandidateExperience? Experience { get; init; }
    public CandidatePricing? Pricing { get; init; }
    public CandidateReputation? Reputation { get; init; }
    public IReadOnlyList<string>? PortfolioHighlights { get; init; }
    public string? MatchContext { get; init; }
}

public sealed class CandidateSkill
{
    public required string Name { get; init; }
    public SkillProficiency Proficiency { get; init; } = SkillProficiency.Intermediate;
    public int? YearsExperience { get; init; }
}

public enum SkillProficiency
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
    Expert = 3
}

public sealed class CandidateExperience
{
    public int TotalProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int? YearsInField { get; init; }
    public IReadOnlyList<string>? RelevantProjects { get; init; }
    public string? SpecialtyDomain { get; init; }
}

public sealed class CandidatePricing
{
    public decimal? HourlyRate { get; init; }
    public decimal? FixedPriceEstimate { get; init; }
    public string? Currency { get; init; } = "USD";
    public bool IsOpenToNegotiation { get; init; } = true;
}

public sealed class CandidateReputation
{
    public double? AverageRating { get; init; }
    public int? TotalReviews { get; init; }
    public int? ResponseTimeHours { get; init; }
    public double? CompletionRate { get; init; }
    public bool? IsVerified { get; init; }
}
