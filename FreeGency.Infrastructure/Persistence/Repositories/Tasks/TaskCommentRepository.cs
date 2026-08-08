
namespace FreeGency.Infrastructure.Persistence.Repositories.Tasks;


public class TaskCommentRepository : GenericRepository<TaskComment>, ITaskCommentRepository
{
    public TaskCommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
