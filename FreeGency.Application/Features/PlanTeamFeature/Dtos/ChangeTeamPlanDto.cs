using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Dtos
{
    public class ChangeTeamPlanDto
    {
        public Guid SubscriptionId { get; set; }
        public Guid PlanId { get; set; }
        public Guid TeamId { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
    }
}
