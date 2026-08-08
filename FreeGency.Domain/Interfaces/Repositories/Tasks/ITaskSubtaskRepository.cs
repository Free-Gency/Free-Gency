
namespace FreeGency.Domain.Interfaces.Repositories.Tasks;


public interface ITaskSubtaskRepository : IGenericRepository<TaskSubtask>
{
    Task<IReadOnlyList<TaskSubtask>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
}
