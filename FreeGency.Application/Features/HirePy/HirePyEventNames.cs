namespace FreeGency.Application.Features.HirePy;

public static class HirePyEventNames
{
    public const string Started = "HirePyStarted";
    public const string ProjectGenerationStarted = "ProjectGenerationStarted";
    public const string ProjectGenerated = "ProjectGenerated";
    public const string ProjectCreated = "ProjectCreated";

    public const string RankingStarted = "HirePyRankingStarted";
    public const string RankingCompleted = "HirePyRankingCompleted";
    public const string TopCandidatesSelected = "HirePyTopCandidatesSelected";
    public const string InvitationsSent = "HirePyInvitationsSent";
    public const string CandidateAccepted = "HirePyCandidateAccepted";
    public const string CandidateDeclined = "HirePyCandidateDeclined";
    public const string RankingFailed = "HirePyRankingFailed";

    public const string DiscussionStarted = "HirePyDiscussionStarted";
    public const string MilestonePlanRequested = "HirePyMilestonePlanRequested";
    public const string DiscussionFailed = "HirePyDiscussionFailed";

    public const string MilestonePlanningStarted = "HirePyMilestonePlanningStarted";
    public const string MilestonePlanReceived = "HirePyMilestonePlanReceived";
    public const string MilestoneReviewStarted = "HirePyMilestoneReviewStarted";
    public const string MilestoneRevisionRequested = "HirePyMilestoneRevisionRequested";
    public const string MilestonePlanFinalized = "HirePyMilestonePlanFinalized";

    public const string CandidateEvaluationStarted = "CandidateEvaluationStarted";
    public const string CandidateEvaluationCompleted = "CandidateEvaluationCompleted";
    public const string CandidateComparisonStarted = "CandidateComparisonStarted";
    public const string RecommendationReady = "RecommendationReady";
    public const string WaitingForClientApproval = "WaitingForClientApproval";
    public const string CandidateEvaluationFailed = "CandidateEvaluationFailed";

    public const string HiringStarted = "HiringStarted";
    public const string HiringCompleted = "HiringCompleted";
    public const string HirePyCompleted = "HirePyCompleted";
}
