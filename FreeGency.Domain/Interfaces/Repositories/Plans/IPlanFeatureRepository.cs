
namespace FreeGency.Domain.Interfaces.Repositories.Plans;

public interface IPlanFeatureRepository : IGenericRepository<PlanFeature>
{
    Task<List<PlanFeature>> GetByPlanIdAsync(
       Guid planId,
       CancellationToken ct = default);
}
