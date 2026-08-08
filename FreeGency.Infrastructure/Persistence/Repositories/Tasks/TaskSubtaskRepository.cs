
namespace FreeGency.Infrastructure.Persistence.Repositories.Tasks;


public class TaskSubtaskRepository : GenericRepository<TaskSubtask>, ITaskSubtaskRepository
{
    public TaskSubtaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskSubtask>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);
    }
}
