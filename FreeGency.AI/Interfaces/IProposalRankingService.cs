using FreeGency.AI.Ranking.ProposalRanking;

namespace FreeGency.AI.Interfaces;

public interface IProposalRankingService
{
    Task<ProjectRankingResponse> RankAsync(ProjectRankingRequest request, CancellationToken ct = default);
}
