using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities.TeamPlans
{
    public class TeamSubscription : ISoftDeletableEntity
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
        public Guid TeamId { get; set; }

        public virtual Team Team { get; set; } = null!;

        public Guid TeamPlanId { get; set; }

        public virtual TeamPlan TeamPlan { get; set; } = null!;

        public BillingPeriod BillingPeriod { get; set; }

        public bool AutoRenew { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public virtual ICollection<TeamUsageRecord> UsageRecords { get; set; }
      = [];
    }
}
