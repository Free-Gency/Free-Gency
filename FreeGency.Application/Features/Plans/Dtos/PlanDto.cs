using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Plans.Dtos
{
    public class PlanDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal MonthlyPrice { get; set; }

        public decimal YearlyPrice { get; set; }

        public bool IsActive { get; set; }

        public List<PlanFeatureDto> Features { get; set; } = [];
    }
}
