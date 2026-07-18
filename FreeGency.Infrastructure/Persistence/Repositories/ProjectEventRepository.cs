using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ProjectEventRepository:GenericRepository<ProjectEvent>,IProjectEventRepository
{
    public ProjectEventRepository(ApplicationDbContext context):base(context)
    {
    }
    public async Task<IEnumerable<ProjectEvent>> GetByProjectIdAsync(Guid projectId,int skip,int take,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().AsSplitQuery().Include(e => e.ActorUser).Include(e => e.Milestone).Where(e => e.ProjectId == projectId).OrderByDescending(e => e.CreatedAt)
            .Skip(skip).Take(take).ToListAsync(ct);
    }
    public async Task<ProjectEvent?> GetLatestByTypeAsync(Guid projectId,EventType eventType,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Include(e => e.ActorUser).Include(e => e.Milestone)
            .Where(e =>e.ProjectId == projectId && e.EventType == eventType)
            .OrderByDescending(e => e.CreatedAt).FirstOrDefaultAsync(ct);
    }
}