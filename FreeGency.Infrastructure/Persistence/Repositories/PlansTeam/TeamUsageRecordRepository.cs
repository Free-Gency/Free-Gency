using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories.PlansTeam
{
    public class TeamUsageRecordRepository:GenericRepository<TeamUsageRecord>,ITeamUsageRecordRepository
    {
        public TeamUsageRecordRepository(ApplicationDbContext context):base(context)
        {
            
        }

        public async Task<List<TeamUsageRecord>> GetTeamUsageRecordAsync()
        {
            return await _dbSet.Where(x => x.Feature == TeamFeatureType.CreateProposal).ToListAsync();
        }

        public async Task<List<TeamUsageRecord>> GetUsageRecordsBySubscriptionIdAsync(
    Guid subscriptionId)
        {
            return await _dbSet
                .Where(x => x.TeamSubscriptionId == subscriptionId)
                .ToListAsync();
        }
        public async Task<List<TeamUsageRecord>> GetByTeamIdAsync(
    Guid subId,
    CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x => x.TeamSubscriptionId == subId)
                .ToListAsync(ct);
        }
    }
}
