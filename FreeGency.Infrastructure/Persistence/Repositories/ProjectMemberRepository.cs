
namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ProjectMemberRepository:GenericRepository<ProjectMember>,IProjectMemberRepository
{
    public ProjectMemberRepository(ApplicationDbContext context):base(context)
    {
    }
    public async Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Where(pm => pm.ProjectId == projectId).OrderBy(pm => pm.AssignedAt).ToListAsync(ct);
    }
    public async Task<bool> IsMemberAsync(Guid projectId,Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(pm=>pm.ProjectId == projectId&&pm.UserId == userId,ct);
    }

    public async Task RemoveAsync(Guid projectId,Guid userId,CancellationToken ct = default)
    {
        var member = await _dbSet.FirstOrDefaultAsync(pm =>pm.ProjectId == projectId && pm.UserId == userId,ct);
        if (member is null)
            return;
        _dbSet.Remove(member);
    }

    public async Task AddSoloAssigneeAsync(Guid projectId,Guid userId,Guid assignedByUserId,CancellationToken ct = default)
    {
        if (await IsMemberAsync(projectId, userId, ct))
            return;
        await _dbSet.AddAsync(new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            RoleInProject = "Owner"
        }, ct);
    }


    public async Task<IEnumerable<ProjectMember>> GetMembersWithUsersAsync(
    Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(pm => pm.User)
            .Include(pm => pm.User.DeveloperProfile)
            .Where(pm => pm.ProjectId == projectId)
            .OrderBy(pm => pm.AssignedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProjectMember>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(pm => pm.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddMemberAsync(
        Guid projectId, Guid userId, string roleInProject,
        Guid assignedByUserId, CancellationToken ct = default)
    {
        if (await IsMemberAsync(projectId, userId, ct))
            return;
        await _dbSet.AddAsync(new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            RoleInProject = roleInProject
        }, ct);
    }
}