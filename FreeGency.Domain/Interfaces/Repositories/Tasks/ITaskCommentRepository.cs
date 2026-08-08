
namespace FreeGency.Domain.Interfaces.Repositories.Tasks;


public interface ITaskCommentRepository : IGenericRepository<TaskComment>
{
    Task<IReadOnlyList<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
}
