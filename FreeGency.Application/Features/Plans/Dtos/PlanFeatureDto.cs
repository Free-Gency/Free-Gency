using FreeGency.Domain.Entities.Plans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Plans.Dtos
{
    public class PlanFeatureDto
    {
        public string Feature { get; set; }

        public int? Limit { get; set; }

        public bool IsEnabled { get; set; }
    }
}
