namespace FreeGency.Domain.Interfaces.Repositories.Plans;

public interface IPlanRepository : IGenericRepository<Plan>
{
    IQueryable<Plan> GetPlans();
    IQueryable<Plan> GetPlanFeatuerId(Guid id);

    Task<Plan?> GetFreePlanAsync(CancellationToken ct = default);
}
