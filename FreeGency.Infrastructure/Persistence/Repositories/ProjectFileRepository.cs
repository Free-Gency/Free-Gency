using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ProjectFileRepository
    : GenericRepository<ProjectFile>, IProjectFileRepository
{
    public ProjectFileRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<ProjectFile>> GetByProjectIdAsync(Guid projectId,FileKind? kind = null,CancellationToken ct = default)
    {
        IQueryable<ProjectFile> query = _dbSet.AsNoTracking().AsSplitQuery().Include(f => f.UploadedByUser).Include(f => f.Milestone).Where(f => f.ProjectId == projectId);
        if (kind.HasValue)
        {
            query = query.Where(f => f.FileKind == kind.Value);
        }

        return await query.OrderByDescending(f => f.CreatedAt).ToListAsync(ct);
    }

    public async Task<IEnumerable<ProjectFile>> GetByMilestoneIdAsync(Guid milestoneId,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking() .Include(f => f.UploadedByUser).Where(f => f.MilestoneId == milestoneId).OrderByDescending(f => f.CreatedAt) .ToListAsync(ct);
    }
}