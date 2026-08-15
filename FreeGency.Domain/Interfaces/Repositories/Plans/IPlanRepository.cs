namespace FreeGency.Domain.Interfaces.Repositories.Plans;

public interface IPlanRepository : IGenericRepository<Entities.Plans.Plan>
{
    IQueryable<Plan> GetPlans();
    IQueryable<Plan> GetPlanFeatuerId(Guid id);
}
