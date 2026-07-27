namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class ProposalSemanticScore
{
    public required string CandidateId { get; init; }
    public required double Score { get; init; }
    public required double Confidence { get; init; }
    public required string Summary { get; init; }
    public required string Reason { get; init; }
    public required IReadOnlyList<string> Strengths { get; init; }
    public required IReadOnlyList<string> Weaknesses { get; init; }
}

public sealed class ProposalSemanticRankingResult
{
    public required IReadOnlyList<ProposalSemanticScore> Candidates { get; init; }
    public string? OverallSummary { get; init; }
}
