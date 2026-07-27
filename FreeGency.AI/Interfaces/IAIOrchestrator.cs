using FreeGency.AI.DTOs;
using FreeGency.AI.Ranking.ProposalRanking;

namespace FreeGency.AI.Interfaces;

public interface IAIOrchestrator
{
    Task<ProjectRankingResponse> RankProjectAsync(ProjectRankingRequest request, CancellationToken ct = default);
    Task<AIResponse<T>> ProcessAsync<T>(AIRequest request, CancellationToken ct = default) where T : class;
    Task<AIResponse<string>> ChatAsync(string prompt, IDictionary<string, string>? context = null, CancellationToken ct = default);
}
