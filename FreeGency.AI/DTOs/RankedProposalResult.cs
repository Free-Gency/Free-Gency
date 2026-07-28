namespace FreeGency.AI.DTOs;

public sealed class RankedProposalResult
{
    public required string CandidateId { get; init; }
    public required string CandidateName { get; init; }
    public required double Score { get; init; }
    public required int Rank { get; init; }
    public string? Reasoning { get; init; }
    public IDictionary<string, double>? ScoreBreakdown { get; init; }
}

public sealed class RankingExplanation
{
    public required IReadOnlyList<RankedProposalResult> Results { get; init; }
    public string? Summary { get; init; }
    public TimeSpan Elapsed { get; init; }
}
