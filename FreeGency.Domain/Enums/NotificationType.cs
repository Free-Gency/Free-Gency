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
    EscrowLocked,
    MilestoneSubmitted,
    MilestoneChangesRequested,
    MilestoneApproved,
    MilestoneReleased,
    NewChatMessage,
    ReviewReminder,
    ProjectPublished
}
