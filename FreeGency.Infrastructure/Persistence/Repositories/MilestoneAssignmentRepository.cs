
namespace FreeGency.Infrastructure.Persistence.Repositories;

public class MilestoneAssignmentRepository : GenericRepository<MilestoneAssignment>, IMilestoneAssignmentRepository
{
    public MilestoneAssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MilestoneAssignment>> GetByMilestoneIdAsync(
        Guid milestoneId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.User.DeveloperProfile)
            .Where(a => a.MilestoneId == milestoneId)
            .OrderBy(a => a.AssignedAt)
            .ToListAsync(ct);
    }

    public async Task RemoveForMilestoneAsync(Guid milestoneId, CancellationToken ct = default)
    {
        var rows = await _dbSet.Where(a => a.MilestoneId == milestoneId).ToListAsync(ct);
        _dbSet.RemoveRange(rows);
    }
}