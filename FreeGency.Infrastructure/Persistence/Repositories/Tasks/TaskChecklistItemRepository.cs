
namespace FreeGency.Infrastructure.Persistence.Repositories.Tasks;


public class TaskChecklistItemRepository : GenericRepository<TaskChecklistItem>, ITaskChecklistItemRepository
{
    public TaskChecklistItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskChecklistItem>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.TaskId == taskId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(ct);
    }
}
