
namespace FreeGency.Domain.Interfaces.Repositories.Tasks;

public interface ITaskAttachmentRepository : IGenericRepository<TaskAttachment>
{
    Task<IReadOnlyList<TaskAttachment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
}
