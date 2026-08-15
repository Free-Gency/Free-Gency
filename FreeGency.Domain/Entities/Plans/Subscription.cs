
namespace FreeGency.Domain.Entities.Plans;

public class Subscription : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid PlanId { get; set; }
    public virtual Plan Plan { get; set; } = null!;

    public SubscriptionStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool AutoRenew { get; set; }
    public virtual ICollection<UsageRecord> UsageRecords { get; set; }
       = [];
}
public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled,
    Trialing
}
