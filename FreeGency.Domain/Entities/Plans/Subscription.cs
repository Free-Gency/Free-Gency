using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FreeGency.Domain.Entities.Plans
{
    public class Subscription
    {

        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public Guid PlanId { get; set; }
        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;

        public SubscriptionStatus Status { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool AutoRenew { get; set; }
    }
    public enum SubscriptionStatus
    {
        Active,
        Expired,
        Cancelled,
        Trialing
    }
}
