using FreeGency.AI.Ranking.ProposalRanking;

namespace FreeGency.Application.Common.Interfaces;

public interface IProposalRankingService
{
    Task<ApiResponse<ProjectRankingResponse>> RankAsync(Guid projectId, int topK = 10, CancellationToken ct = default);
}
