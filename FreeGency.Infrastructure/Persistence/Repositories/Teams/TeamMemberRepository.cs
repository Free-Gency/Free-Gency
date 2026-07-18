
using Microsoft.EntityFrameworkCore;


namespace FreeGency.Infrastructure.Persistence.Repositories.Teams;

public sealed class TeamMemberRepository : GenericRepository<TeamMember>, ITeamMemberRepository
{
    public TeamMemberRepository(ApplicationDbContext context) : base(context) { }



    public async Task<IReadOnlyList<TeamMember>> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(tm => tm.TeamId == teamId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TeamMember>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .ToListAsync(ct);
    }


    public async Task<TeamMember?> GetSingleInTeamAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(tm => tm.TeamId == teamId && tm.UserId == userId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetMemberCountAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(tm => tm.TeamId == teamId)
            .CountAsync(ct);
    }


    public async Task<bool> HasRoleAsync(Guid teamId, Guid userId, Role role, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId && tm.TeamRole == role, ct);
    }


    public Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        return _dbSet.AsNoTracking().AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId, ct);
    }

    public Task<bool> IsLeaderAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        return _dbSet.AsNoTracking().AnyAsync(
            tm => tm.TeamId == teamId && tm.UserId == userId && tm.TeamRole == Role.TeamLeader,
            ct);
    }


    public async Task<IReadOnlyList<TeamMember>> GetLeadersAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(tm => tm.TeamId == teamId && tm.TeamRole == Role.TeamLeader)
            .ToListAsync(ct);
    }



    public async Task RemoveAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        var member = await _dbSet.FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, ct);
        if (member is not null)
            Delete(member);
    }

    public async Task RemoveByTeamIdAsync(Guid teamId, CancellationToken ct = default)
    {
        var members = await _dbSet.Where(tm => tm.TeamId == teamId).ToListAsync(ct);
        if (members.Any())
            await _dbSet.Where(tm => tm.TeamId == teamId).ExecuteDeleteAsync(ct);
    }

}
