
namespace FreeGency.Domain.Interfaces.Repositories;


public interface IMilestoneAssignmentRepository : IGenericRepository<MilestoneAssignment>
{
    Task<IReadOnlyList<MilestoneAssignment>> GetByMilestoneIdAsync(Guid milestoneId, CancellationToken ct = default);
    Task RemoveForMilestoneAsync(Guid milestoneId, CancellationToken ct = default);
}
