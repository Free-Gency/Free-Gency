using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class HiringAgentRun : ISoftDeletableEntity
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
    public Guid ClientUserId { get; set; }

    public HiringAgentRunStatus Status { get; set; } = HiringAgentRunStatus.Queued;
    public int TopK { get; set; } = 5;

    public DateTime InviteDeadlineUtc { get; set; }
    public DateTime DiscussionDeadlineUtc { get; set; }

    public Guid? RecommendedProposalId { get; set; }
    public Guid? RecommendedPlanVersionId { get; set; }
    public Guid? RecommendedCandidateId { get; set; }

    public string? ReportJson { get; set; }
    public string? FailureReason { get; set; }

    public DateTime? ReportReadyAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Set when the client confirms hire once. After this, the agent may auto-review
    /// revised plans and accept without another Confirm Hire click.
    /// </summary>
    public DateTime? ClientHireApprovedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual User ClientUser { get; set; } = null!;
    public virtual ProjectProposal? RecommendedProposal { get; set; }
    public virtual MilestonePlanVersion? RecommendedPlanVersion { get; set; }
    public virtual HiringAgentCandidate? RecommendedCandidate { get; set; }
    public virtual ICollection<HiringAgentCandidate> Candidates { get; set; } = new List<HiringAgentCandidate>();
}
