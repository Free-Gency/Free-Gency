using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities.TeamPlans
{
    public class TeamUsageRecord : ISoftDeletableEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public Guid TeamSubscriptionId { get; set; }

        public virtual TeamSubscription Subscription { get; set; } = null!;

        public TeamFeatureType Feature { get; set; }

        public int Used { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }
    }
}
