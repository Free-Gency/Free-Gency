using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class MilestonePlanVersion : ISoftDeletableEntity
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
    public Guid ProposalId { get; set; }
    public int Version { get; set; }
    public PlanVersionStatus Status { get; set; } = PlanVersionStatus.Proposed;
    public string? ChangeComment { get; set; }
    public Guid ProposedByUserId { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual ProjectProposal Proposal { get; set; } = null!;
    public virtual ICollection<MilestonePlanItem> Items { get; set; } = [];
}
