
namespace FreeGency.Domain.Enums;

public enum EventType
{
    ProposalAccepted=0,
    MilestonePlanProposed = 1,
    MilestonePlanChangesRequested,
    MilestonePlanAgreed,
    EscrowLocked,
    MemberAdded,
    MemberRemoved,
    FileUploaded, 
    MilestoneSubmitted,
    MilestoneChangesRequested,
    MilestoneApproved,
    MilestoneReleased,
    ProjectCompleted,

    TaskCreated = 20,
    TaskAssigned = 21,
    TaskStatusChanged = 22,
    TaskCommentAdded = 23
}
