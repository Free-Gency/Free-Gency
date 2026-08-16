using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Dtos
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }

        public Guid TeamPlanId { get; set; }

        public string PlanName { get; set; } = string.Empty;

        public BillingPeriod BillingPeriod { get; set; }

        public bool AutoRenew { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public decimal Price { get; set; }
        public List<PlanFeatuerTeamDto> Features { get; set; } = [];
    }
}
