using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;


namespace FreeGency.Infrastructure.Persistence.Repositories.PlansTeam;

public class TeamPlanRepository:GenericRepository<TeamPlan>,ITeamPlanRepository
{
    public TeamPlanRepository(ApplicationDbContext dbContext):base(dbContext)
    {
        
    }

    public IQueryable<TeamPlan> GetTeamPlanQueryable()
    {
        return _dbSet.AsQueryable().Include(tp => tp.Features).AsQueryable();
    }
   
}
