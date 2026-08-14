using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FreeGency.Domain.Entities.Plans
{
    public class UsageRecord
    {
        public Guid Id { get; set; }

        public Guid SubscriptionId { get; set; }
        [ForeignKey(nameof(SubscriptionId))]
        public Subscription subscription { get; set; } = null!;

        public FeatureType Feature { get; set; }

        public int Used { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }
    }
}
