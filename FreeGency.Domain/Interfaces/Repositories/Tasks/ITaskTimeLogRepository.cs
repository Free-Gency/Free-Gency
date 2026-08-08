
namespace FreeGency.Domain.Interfaces.Repositories.Tasks;


public interface ITaskTimeLogRepository : IGenericRepository<TaskTimeLog>
{
    Task<IReadOnlyList<TaskTimeLog>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<decimal> SumHoursByTaskAsync(Guid taskId, CancellationToken ct = default);
}
