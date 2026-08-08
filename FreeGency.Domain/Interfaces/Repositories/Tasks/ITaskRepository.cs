
namespace FreeGency.Domain.Interfaces.Repositories.Tasks;


public interface ITaskRepository : IGenericRepository<ProjectTask>
{
    Task<IReadOnlyList<ProjectTask>> GetByMilestoneIdAsync(Guid milestoneId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTask>> GetAssignedToUserAsync(Guid userId, CancellationToken ct = default);
    Task<ProjectTask?> GetByIdWithDetailsAsync(Guid taskId, CancellationToken ct = default);
    Task<int> CountIncompleteAsync(Guid milestoneId, CancellationToken ct = default);
}
