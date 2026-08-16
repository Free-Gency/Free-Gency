using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities.TeamPlans
{
    public class TeamPlanFeature : ISoftDeletableEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public Guid TeamPlanId { get; set; }

        public virtual TeamPlan TeamPlan { get; set; } = null!;

        public TeamFeatureType Feature { get; set; }

        public int? Limit { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
    public enum TeamFeatureType
    {
        CreateProposal
    }
}
