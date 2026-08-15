using FreeGency.Application.Features.Plans.Dtos;
using FreeGency.Application.Features.Plans.Mapping;
using FreeGency.Domain.Entities.Plans;
using FreeGency.Domain.Interfaces.Repositories.Plans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Plans.Commands
{
    public partial class PlanService(IUnitOfWork unitOfWork) : IPlanService
    {
        private readonly IPlanRepository planRepository = unitOfWork.Repository<IPlanRepository, Plan>();

     

        public async Task<Result<List<PlanDto>>> GetPlans()
        {
            var plans = await planRepository.GetPlans().Select(x=>x.ToDto()).ToListAsync();
            return Result.Success(plans);
        }
    }
}