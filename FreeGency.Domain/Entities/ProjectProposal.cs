using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class ProjectProposal : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ProjectId { get; set; }
    public ApplicantType ApplicantType { get; set; }
    public Guid? TeamId { get; set; }
    /// <summary>
    /// Solo applicant, or the team leader who submitted (negotiation speaker) when ApplicantType is Team.
    /// </summary>
    public Guid? UserId { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public string Approach { get; set; } = string.Empty;
    public string? ProposedTimeline { get; set; }
    public string? SimilarLinksUrl { get; set; }
    public decimal ProposedBudget { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public string? RejectReason { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ResponseAt { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual Team? Team { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<ProposalAttachment> ProposalAttachments { get; set; } = [];
    public virtual ChatRoom? ChatRoom { get; set; }
    public virtual ICollection<MilestonePlanVersion> MilestonePlanVersions { get; set; } = [];
}
