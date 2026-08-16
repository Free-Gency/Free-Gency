using FreeGency.Domain.Entities.TeamPlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories.plansTeam
{
    public interface ITeamPlanFeatureRepository:IGenericRepository<TeamPlanFeature>
    {
        Task<List<TeamPlanFeature>> GetByPlanIdAsync(Guid planId);

    }
}
