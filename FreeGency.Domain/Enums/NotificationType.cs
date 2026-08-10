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
}
