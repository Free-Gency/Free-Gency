namespace FreeGency.AI.Ranking.ProposalRanking;

public interface IProposalRuleEngine
{
    ProjectRankingResponse Rank(ProjectRankingRequest request);
}
