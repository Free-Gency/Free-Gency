using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities.Plans
{
    public class Plan
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "Free";
        public string? Description { get; set; }


        public decimal MonthlyPrice { get; set; }

        public decimal YearlyPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<PlanFeature> Features { get; set; }
            = new List<PlanFeature>();
    }
}
