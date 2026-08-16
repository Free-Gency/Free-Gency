using FreeGency.Application.Features.PlanTeamFeature.Dtos;
using FreeGency.Application.Features.PlanTeamFeature.Mapping;
using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Commands
{
    public partial class PlanTeamService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
    {
        private readonly ITeamPlanRepository teamPlanRepository = unitOfWork.Repository<ITeamPlanRepository, TeamPlan>();
        private readonly ITeamSubscriptionRepository teamSubscriptionRepository = unitOfWork.Repository<ITeamSubscriptionRepository, TeamSubscription>();
        private readonly ITeamUsageRecordRepository teamUsageRecordRepository = unitOfWork.Repository<ITeamUsageRecordRepository, TeamUsageRecord>();

        public async Task<Result<List<PlanTeamDto>>> GetPlansTeam()
        {
            var plans = await teamPlanRepository.GetTeamPlanQueryable().Select(x => x.ToDto()).ToListAsync();
            return Result.Success(plans);
        }
    }
}
