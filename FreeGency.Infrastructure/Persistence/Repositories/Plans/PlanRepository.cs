
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class PlanRepository : GenericRepository<Plan>, IPlanRepository
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


    public async Task<Plan?> GetFreePlanAsync(CancellationToken ct = default)
    => await _dbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Name == "Free", ct);

}
