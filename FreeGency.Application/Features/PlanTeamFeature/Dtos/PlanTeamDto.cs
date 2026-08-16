using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Dtos
{
    public class PlanTeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public decimal MonthlyPrice { get; set; }

        public decimal YearlyPrice { get; set; }

        public List<PlanFeatuerTeamDto> Features { get; set; } = [];
    }
}
