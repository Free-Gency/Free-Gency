namespace FreeGency.Domain.Enums;

public enum MessageType
{
    Text = 0,
    Attachment = 1,
    System = 2,
    MilestonePlan = 3,
    /// <summary>Developer submitted a milestone for client review.</summary>
    WorkSubmitted = 4,
    /// <summary>Client requested changes on submitted work.</summary>
    WorkChangesRequested = 5,
    /// <summary>Client approved and released milestone funds.</summary>
    MilestoneReleased = 6,
    /// <summary>Client funded the next milestone (work can start).</summary>
    MilestoneFunded = 7,
}
