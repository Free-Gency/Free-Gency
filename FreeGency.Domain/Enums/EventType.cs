using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Enums
{
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
        ProjectCompleted
    }
}
