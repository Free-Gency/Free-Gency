
namespace FreeGency.Infrastructure.Persistence.Repositories.Tasks;


public class TaskTimeLogRepository : GenericRepository<TaskTimeLog>, ITaskTimeLogRepository
{
    public TaskTimeLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskTimeLog>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.TaskId == taskId)
            .OrderByDescending(l => l.WorkDate)
            .ToListAsync(ct);
    }

    public async Task<decimal> SumHoursByTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.TaskId == taskId)
            .SumAsync(l => (decimal?)l.Hours, ct) ?? 0;
    }
}
