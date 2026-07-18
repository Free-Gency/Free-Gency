
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Teams;

public sealed class TeamJoinRequestRepository : GenericRepository<TeamJoinRequest>, ITeamJoinRequestRepository
{
    public TeamJoinRequestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TeamJoinRequest>> GetPendingByTeamIdAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.TeamId == teamId && r.Status == TeamJoinRequestStatus.pending)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }


    public async Task<IReadOnlyList<TeamJoinRequest>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<TeamJoinRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Include(r => r.User)
            .Include(r => r.TeamJob)
            .FirstOrDefaultAsync(ct);
    }


    public async Task<bool> HasPendingRequestAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(
            r => r.TeamId == teamId && r.UserId == userId && r.Status == TeamJoinRequestStatus.pending,
            ct);
    }

    public async Task UpdateStatusAsync(Guid id, TeamJoinRequestStatus status, DateTime responseAt, string respondedByUserId, CancellationToken ct = default)
    {
        await _dbSet
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.Status, status)
                    .SetProperty(r => r.ResponseAt, responseAt)
                    .SetProperty(r => r.RespondedByUserId, respondedByUserId),
                ct);
    }
}
