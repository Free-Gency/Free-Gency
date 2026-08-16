using FreeGency.Application.Features.PlanTeamFeature.Dtos;
using FreeGency.Domain.Entities.TeamPlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Mapping
{
    public static class TeamPlanMapping
    {
        public static PlanTeamDto ToDto(this TeamPlan teamPlan)
        {
            return new PlanTeamDto{
                Id=teamPlan.Id,
                Name=teamPlan.Name,
                MonthlyPrice=teamPlan.MonthlyPrice.Value,
                YearlyPrice=teamPlan.YearlyPrice.Value,
                Features=teamPlan.Features.Select((x=>new PlanFeatuerTeamDto
                {
                    Feature=x.Feature.ToString(),
                    IsEnabled=x.IsEnabled,
                    Limit=x.Limit
                })).ToList()
            };
        }
    }
}
