using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories.PlansTeam
{
    public class TeamSubscriptionRepository:GenericRepository<TeamSubscription>,ITeamSubscriptionRepository
    {
        public TeamSubscriptionRepository(ApplicationDbContext context):base(context)
        {
            
        }
        public async Task<TeamSubscription?> GetByTeamIdAsync(Guid teamId)
        {
            return await _dbSet
                .Include(x => x.TeamPlan)
                .FirstOrDefaultAsync(x => x.TeamId == teamId);
        }
    }
}
