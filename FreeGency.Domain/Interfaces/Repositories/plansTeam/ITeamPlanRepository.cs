using FreeGency.Domain.Entities.TeamPlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories.plansTeam
{
    public interface ITeamPlanRepository:IGenericRepository<TeamPlan>
    {
        IQueryable<TeamPlan> GetTeamPlanQueryable();
    }
    
}
