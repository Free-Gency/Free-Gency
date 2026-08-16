using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories.PlansTeam
{
    public class TeamPlanRepository:GenericRepository<TeamPlan>,ITeamPlanRepository
    {
        public TeamPlanRepository(ApplicationDbContext dbContext):base(dbContext)
        {
            
        }

        public IQueryable<TeamPlan> GetTeamPlanQueryable()
        {
            return _dbSet.AsQueryable();
        }
       
    }
}
