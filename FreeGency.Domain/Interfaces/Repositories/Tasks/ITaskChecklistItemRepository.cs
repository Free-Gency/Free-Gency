
namespace FreeGency.Domain.Interfaces.Repositories.Tasks;


public interface ITaskChecklistItemRepository : IGenericRepository<TaskChecklistItem>
{
    Task<IReadOnlyList<TaskChecklistItem>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
}
