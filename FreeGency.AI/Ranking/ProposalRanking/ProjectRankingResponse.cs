namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class ProjectRankingResponse
{
    public required string ProjectId { get; init; }
    public required IReadOnlyList<RankedProposal> RankedProposals { get; init; }
    public required RankingMetadata Metadata { get; init; }
    public string? AiSummary { get; init; }
}

public sealed class RankingMetadata
{
    public int TotalCandidatesEvaluated { get; init; }
    public int ReturnedCount { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public bool UsedAiEmbeddings { get; init; }
    public bool FromCache { get; init; }
    public string? ModelUsed { get; init; }
    public IReadOnlyList<string>? Warnings { get; init; }
}
