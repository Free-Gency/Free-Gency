namespace FreeGency.Domain.Enums;

public enum NotificationType
{
    System = 0,
    NewProposal = 1,
    ProposalAccepted,
    ProposalRejected,
    JoinRequestReceived,
    JoinRequestAccepted,
    JoinRequestRejected,
    MilestonePlanProposed,
    MilestonePlanChangesRequested,
    MilestonePlanAgreed,
    MilestoneFunded,
    EscrowLocked,
    MilestoneSubmitted,
    MilestoneChangesRequested,
    MilestoneApproved,
    MilestoneReleased,
    NewChatMessage,
    ReviewReminder,
    ProjectPublished,
    Wallet,

    TaskAssigned = 100,
    TaskStatusChanged = 101,
    TaskCommentAdded = 102,

    InviteReceived = 110,
    InviteAccepted = 111,
    InviteRejected = 112,

    HirePyProjectCreated = 120,
    HirePyNoCandidatesFound = 121,
    HirePyInvitationsSent = 122,
    HirePyFailed = 123,
    HirePyDiscussionStarted = 124,
    HirePyMilestonePlanRequested = 125,
    HirePyMilestonePlanFinalized = 126,
    HirePyRecommendationReady = 127,
    HirePyHiringCompleted = 128,
}
