
using FreeGency.Domain.Interfaces.Repositories.Plans;

namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class PlanRepository : GenericRepository<Domain.Entities.Plans.Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public IQueryable<Plan> GetPlanFeatuerId(Guid id)
    {
        return _dbSet.Where(x => x.Id == id);
    }

    public IQueryable<Plan> GetPlans()
    {
        return _dbSet.AsQueryable();
    }
}
