using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class MilestonePlanVersionRepository
    : GenericRepository<MilestonePlanVersion>, IMilestonePlanVersionRepository
{
    public MilestonePlanVersionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MilestonePlanVersion>> GetByProjectIdAsync(
        Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(v => v.Items.OrderBy(i => i.SortOrder))
            .Where(v => v.ProjectId == projectId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MilestonePlanVersion>> GetByProposalIdAsync(
        Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(v => v.Items.OrderBy(i => i.SortOrder))
            .Where(v => v.ProposalId == proposalId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(ct);
    }

    public async Task<MilestonePlanVersion?> GetLatestByProjectIdAsync(
        Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.Items.OrderBy(i => i.SortOrder))
            .Where(v => v.ProjectId == projectId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MilestonePlanVersion?> GetLatestByProposalIdAsync(
        Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.Items.OrderBy(i => i.SortOrder))
            .Where(v => v.ProposalId == proposalId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MilestonePlanVersion?> GetByIdWithItemsAsync(
        Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.Items.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().CountAsync(v => v.ProjectId == projectId, ct);
    }

    public async Task<int> CountByProposalIdAsync(Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().CountAsync(v => v.ProposalId == proposalId, ct);
    }
}
