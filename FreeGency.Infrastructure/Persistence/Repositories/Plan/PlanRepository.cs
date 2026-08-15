
namespace FreeGency.Infrastructure.Persistence.Repositories.Plan;

public class PlanRepository : GenericRepository<Domain.Entities.Plans.Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext context) : base(context)
    {
    }
}
