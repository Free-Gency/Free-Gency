using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class HirePyInterviewRepository
    : GenericRepository<HirePyInterview>, IHirePyInterviewRepository
{
    public HirePyInterviewRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<List<HirePyInterview>> GetPendingAsync(CancellationToken ct = default)
    {
        var staleBefore = DateTime.UtcNow.AddMinutes(-2);
        var activeStatuses = new[]
        {
            HirePyInterviewStatus.Started,
            HirePyInterviewStatus.MilestonePlanRequested,
            HirePyInterviewStatus.MilestonePlanReceived,
            HirePyInterviewStatus.MilestoneRevisionRequested
        };
        return await _dbSet
            .Where(i => !i.IsDeleted
                && activeStatuses.Contains(i.Status)
                && (i.ProcessingAt == null || i.ProcessingAt < staleBefore))
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<HirePyInterview?> GetByProposalIdAsync(Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ProjectProposalId == proposalId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<HirePyInterview>> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.HirePySessionId == sessionId && !i.IsDeleted)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(ct);
    }
}
