using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class EscrowHold : ISoftDeletableEntity
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
    public decimal TotalAmount { get; set; } = 0;
    public decimal TotalReleased { get; set; } = 0;
    public FundingStatus FundingStatus { get; set; } = FundingStatus.Unlocked;
    public PlanStatus planStatus { get; set; } = PlanStatus.AwaitingPlan;
    public DateTime? LockedAt { get; set; }
    public DateTime? PlanAgreedAt { get; set; }

    public Project Project { get; set; } = null!;
}
