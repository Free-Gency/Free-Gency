namespace FreeGency.Domain.Enums;

public enum HirePySessionStatus
{
    Draft = 0,
    GeneratingProject,
    ProjectCreated,
    RankingCandidates,
    TopCandidatesSelected,
    InvitingCandidates,
    WaitingForCandidates,
    CandidateDiscussion,
    MilestonePlanning,
    CandidateEvaluation,
    CandidateComparison,
    RecommendationReady,
    WaitingForClientApproval,
    Hiring,
    Completed,
    Failed,
    Cancelled
}
