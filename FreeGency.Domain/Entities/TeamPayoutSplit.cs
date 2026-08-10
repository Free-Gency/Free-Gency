using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class TeamPayoutSplit : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid TeamId { get; set; }
    public Guid? ProjectId { get; set; }
    /// <summary>Null = team/project-level split. Set = milestone-specific payout share.</summary>
    public Guid? MilestoneId { get; set; }
    public Guid UserId { get; set; }
    public SplitType SplitType { get; set; }
    public decimal Value { get; set; }

    public virtual Team Team { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual Milestone? Milestone { get; set; }
    public virtual User User { get; set; } = null!;
}
