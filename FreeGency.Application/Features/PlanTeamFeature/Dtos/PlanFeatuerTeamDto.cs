using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Dtos
{
    public class PlanFeatuerTeamDto
    {
        public string Feature { get; set; }

        public int? Limit { get; set; }

        public bool IsEnabled { get; set; }
    }
}
