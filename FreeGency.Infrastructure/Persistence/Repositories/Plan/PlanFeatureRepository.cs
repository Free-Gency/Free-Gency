

namespace FreeGency.Infrastructure.Persistence.Repositories.Plan;

public class PlanFeatureRepository : GenericRepository<PlanFeature>, IPlanFeatureRepository
{
    public PlanFeatureRepository(ApplicationDbContext context) : base(context)
    {
    }
}
