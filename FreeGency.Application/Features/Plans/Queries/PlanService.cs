using FreeGency.Application.Features.Plans.Mapping;


namespace FreeGency.Application.Features.Plans.Commands;

public partial class PlanService(IUnitOfWork unitOfWork) : IPlanService
{
    private readonly IPlanRepository planRepository = unitOfWork.Repository<IPlanRepository, Plan>();

 

    public async Task<Result<List<PlanDto>>> GetPlans()
    {
        var plans = await planRepository.GetPlans().ToListAsync();
        return Result.Success(plans.Select(p => p.ToDto()).ToList());
    }
}