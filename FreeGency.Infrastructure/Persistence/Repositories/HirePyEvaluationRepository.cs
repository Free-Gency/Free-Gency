using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class HirePyEvaluationRepository
    : GenericRepository<HirePyEvaluation>, IHirePyEvaluationRepository
{
    public HirePyEvaluationRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<HirePyEvaluation>> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.HirePySessionId == sessionId && !e.IsDeleted)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HirePyEvaluation>> GetByProposalIdAsync(
        Guid proposalId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.ProjectProposalId == proposalId && !e.IsDeleted)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync(ct);
    }
}
