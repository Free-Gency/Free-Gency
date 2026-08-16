using FreeGency.Application.Features.Plans.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IPlanService
    {
        Task<Result<List<PlanDto>>> GetPlans();
        Task<Result<PlanDto>> ChangeSubscriptionAsync(Guid userId, ChangePlanRequestDto request, CancellationToken ct = default);
    }
}
