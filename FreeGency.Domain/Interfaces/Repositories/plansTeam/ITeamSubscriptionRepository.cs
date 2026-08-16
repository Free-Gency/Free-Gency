using FreeGency.Domain.Entities.TeamPlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories.plansTeam
{
    public interface ITeamSubscriptionRepository:IGenericRepository<TeamSubscription>
    {
        Task<TeamSubscription?> GetByTeamIdAsync(Guid teamId);
    }
}
