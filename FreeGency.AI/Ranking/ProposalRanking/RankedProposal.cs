namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class RankedProposal
{
    public required string CandidateId { get; init; }
    public required string CandidateName { get; init; }
    public required int Rank { get; init; }
    public required double OverallScore { get; init; }
    public required ScoreBreakdown ScoreBreakdown { get; init; }
    public string? AiReasoning { get; init; }
    public MatchSummary? MatchSummary { get; init; }
}

public sealed class ScoreBreakdown
{
    public double SkillMatch { get; init; }
    public double ExperienceRelevance { get; init; }
    public double ReputationScore { get; init; }
    public double BudgetFit { get; init; }
    public double AvailabilityFit { get; init; }
    public double ProposalQuality { get; init; }
    public double AiSemanticScore { get; init; }
    public double WeightedTotal { get; init; }
}

public sealed class MatchSummary
{
    public int MatchedRequiredSkills { get; init; }
    public int TotalRequiredSkills { get; init; }
    public int MatchedPreferredSkills { get; init; }
    public int TotalPreferredSkills { get; init; }
    public IReadOnlyList<string>? MissingSkills { get; init; }
    public string? FitVerdict { get; init; }
}
