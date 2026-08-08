
namespace FreeGency.Infrastructure.Persistence.Repositories.Tasks;


public class TaskAttachmentRepository : GenericRepository<TaskAttachment>, ITaskAttachmentRepository
{
    public TaskAttachmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskAttachment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.TaskId == taskId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
