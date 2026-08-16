using FreeGency.Application.Features.PlanTeamFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IPlanTeamService
    {
        Task<Result<List<PlanTeamDto>>> GetPlansTeam();
        Task<Result<bool>> ToggleAutoRenew(Guid SubId);
        Task<Result> ChangeMyPlan(ChangeTeamPlanDto dto);
        Task<SubscriptionDto> GetMySubscription(Guid teamId);
    }
}
