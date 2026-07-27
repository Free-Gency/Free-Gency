namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class ScoringWeights
{
    public double SkillMatch { get; init; } = 40;
    public double BudgetMatch { get; init; } = 15;
    public double CompletedProjects { get; init; } = 15;
    public double Rating { get; init; } = 20;
    public double ProposalQuality { get; init; } = 10;

    public double Total => SkillMatch + BudgetMatch + CompletedProjects + Rating + ProposalQuality;
}
