using FreeGency.Domain.Entities.TeamPlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories.plansTeam
{
    public interface ITeamUsageRecordRepository:IGenericRepository<TeamUsageRecord>
    {
        Task<List<TeamUsageRecord>> GetUsageRecordsBySubscriptionIdAsync(
       Guid subscriptionId);
    }
}
