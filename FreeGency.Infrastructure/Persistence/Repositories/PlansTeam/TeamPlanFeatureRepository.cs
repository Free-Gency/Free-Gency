using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories.PlansTeam
{
    public class TeamPlanFeatureRepository:GenericRepository<TeamPlanFeature>,ITeamPlanFeatureRepository
    {
        public TeamPlanFeatureRepository(ApplicationDbContext context):base(context)
        {
            
        }
        public async Task<List<TeamPlanFeature>> GetByPlanIdAsync(Guid planId)
        {
            return await _dbSet
                .Where(x => x.TeamPlanId == planId)
                .ToListAsync();
        }
    }
}
