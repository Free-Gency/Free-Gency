
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Teams;

public sealed class TeamJobRepository : GenericRepository<TeamJob>, ITeamJobRepository
{
    public TeamJobRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TeamJob>> GetOpenByTeamIdAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(j => j.TeamId == teamId && j.Status == TeamJobStatus.open)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TeamJob>> GetOpenJobsAsync(Guid? teamId = null, IEnumerable<Guid>? skillIds = null, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(j => j.Status == TeamJobStatus.open);

        if (teamId.HasValue)
            query = query.Where(j => j.TeamId == teamId.Value);

        if (skillIds is not null)
        {
            var skillIdList = skillIds.ToList();
            if (skillIdList.Count > 0)
            {
                query = query.Where(j =>
                    j.TeamJobSkills.Any(tjs => skillIdList.Contains(tjs.SkillId)));
            }
        }

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }


    public async Task<TeamJob?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(j => j.Id == id)
            .Include(j => j.Team)
            .Include(j => j.TeamJobSkills).ThenInclude(tjs => tjs.Skill)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<TeamJob>> GetAllByTeamIdAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(j => j.TeamId == teamId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TeamJob>> GetAllByTeamIdWithDetailsAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(j => j.TeamId == teamId)
            .Include(j => j.Team)
            .Include(j => j.TeamJobSkills).ThenInclude(tjs => tjs.Skill)
            .ToListAsync(ct);
    }

    public async Task AddWithSkillsAsync(TeamJob job, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(job, ct);

        foreach (var skillId in skillIds)
        {
            await _context.Set<TeamJobSkill>().AddAsync(new TeamJobSkill
            {
                Id = Guid.NewGuid(),
                TeamJobId = job.Id,
                SkillId = skillId
            }, ct);
        }
    }

    public async Task CloseAsync(Guid id, DateTime closedAt, CancellationToken ct = default)
    {
        await _dbSet
            .Where(j => j.Id == id)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(j => j.Status, TeamJobStatus.closed)
                    .SetProperty(j => j.ClosedAt, closedAt),
                ct);
    }
}
