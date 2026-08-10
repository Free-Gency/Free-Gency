namespace FreeGency.Domain.Enums;

public enum ProposalStatus
{
    Pending = 0,
    Viewed = 1,
    InDiscussion = 2,
    Rejected = 3,
    Withdrawn = 4,
    Expired = 5,
    /// <summary>Hired — milestone plan accepted for this proposal.</summary>
    Accepted = 6,
}
