

namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class PlanFeatureRepository : GenericRepository<PlanFeature>, IPlanFeatureRepository
{
    public PlanFeatureRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<List<PlanFeature>> GetByPlanIdAsync(
    Guid planId,
    CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.PlanId == planId && x.IsEnabled)
            .ToListAsync(ct);
    }
}
