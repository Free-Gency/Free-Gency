
namespace FreeGency.Infrastructure.Persistence.Repositories.Tasks;

public class TaskRepository : GenericRepository<ProjectTask>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ProjectTask>> GetByMilestoneIdAsync(Guid milestoneId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Include(t => t.Subtasks)
            .Where(t => t.MilestoneId == milestoneId)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Milestone)
            .Include(t => t.Assignee)
            .Include(t => t.Subtasks)
            .Where(t => t.Milestone.ProjectId == projectId)
            .OrderBy(t => t.Milestone.SortOrder)
            .ThenByDescending(t => t.Priority)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetAssignedToUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Milestone)
                .ThenInclude(m => m!.Project)
            .Include(t => t.Subtasks)
            .Where(t => t.AssigneeUserId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<ProjectTask?> GetByIdWithDetailsAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Milestone)
                .ThenInclude(m => m!.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.ChecklistItems)
            .Include(t => t.Attachments)
            .Include(t => t.TimeLogs)
                .ThenInclude(l => l.User)
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);
    }

    public async Task<int> CountIncompleteAsync(Guid milestoneId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(t => t.MilestoneId == milestoneId && t.Status != Domain.Enums.TaskStatus.Done, ct);
    }
}