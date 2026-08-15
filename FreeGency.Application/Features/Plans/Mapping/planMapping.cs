using FreeGency.Application.Features.Plans.Dtos;
using FreeGency.Domain.Entities.Plans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Plans.Mapping
{
    public static class planMapping
    {
        public static PlanDto ToDto(this Plan plan)
        {
            return new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice,
                IsActive=plan.IsActive,
                Features=plan.Features.Select(x=>new PlanFeatureDto
                {
                    Feature=x.Feature.ToString(),
                    Limit=x.Limit,
                    IsEnabled=x.IsEnabled
                }).ToList()
            };
        }
  
    }
}
