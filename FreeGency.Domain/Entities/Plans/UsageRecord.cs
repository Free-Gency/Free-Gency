
namespace FreeGency.Domain.Entities.Plans;

public class UsageRecord: ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid SubscriptionId { get; set; }
    public virtual Subscription subscription { get; set; } = null!;

    public FeatureType Feature { get; set; }

    public int Used { get; set; }

    public long TokensUsed { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }
}
