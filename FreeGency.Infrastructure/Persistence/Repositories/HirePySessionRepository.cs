using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class HirePySessionRepository
    : GenericRepository<HirePySession>, IHirePySessionRepository
{
    public HirePySessionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<HirePySession>> GetByClientUserIdAsync(
        Guid clientUserId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.ClientUserId == clientUserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<HirePySession?> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken ct = default)
    {
        // Tracked: callers (accept/decline/evaluation) mutate Status and SaveChanges.
        return await _dbSet
            .Where(s => s.ProjectId == projectId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<HirePySession>> GetPendingEvaluationAsync(CancellationToken ct = default)
    {
        var statuses = new[]
        {
            HirePySessionStatus.CandidateDiscussion,
            HirePySessionStatus.MilestonePlanning,
            HirePySessionStatus.CandidateEvaluation,
            HirePySessionStatus.CandidateComparison,
            HirePySessionStatus.RecommendationReady
        };
        return await _dbSet
            .AsNoTracking()
            .Where(s => !s.IsDeleted && statuses.Contains(s.Status))
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> TryTransitionStatusAsync(
        Guid sessionId,
        HirePySessionStatus from,
        HirePySessionStatus to,
        CancellationToken ct = default)
    {
        var updated = await _dbSet
            .Where(s => s.Id == sessionId && s.Status == from)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, to)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        return updated == 1;
    }
}
