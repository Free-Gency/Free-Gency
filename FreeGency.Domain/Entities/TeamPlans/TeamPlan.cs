using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities.TeamPlans
{
    public class TeamPlan : ISoftDeletableEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal? MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }

        public virtual ICollection<TeamPlanFeature> Features { get; set; } = [];
    }
}
