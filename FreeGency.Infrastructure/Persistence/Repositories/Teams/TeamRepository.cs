using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Teams;

public sealed class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDbContext context) : base(context) { }


    public async Task<IReadOnlyList<Team>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.OwnerUserId == ownerUserId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Team>> GetByOwnerUserIdWithDetailsAsync(Guid ownerUserId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.OwnerUserId == ownerUserId)
            .Include(t => t.Owner)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User)
            .Include(t => t.SocialLinks)
            .ToListAsync(ct);
    }

    public async Task<Team?> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TeamCode == teamCode, ct);
    }

    public async Task<Team?> GetByTeamCodeWithDetailsAsync(string teamCode, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Owner)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User)
            .Include(t => t.SocialLinks)
            .FirstOrDefaultAsync(t => t.TeamCode == teamCode, ct);
    }


    public async Task<Team?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Owner)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User)
            .Include(t => t.SocialLinks)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }




    public async Task AddWithTaxonomyAsync(Team team, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(team, ct);

        foreach (var (categoryId, isPrimary) in categories)
        {
            await _context.Set<TeamCategory>().AddAsync(new TeamCategory
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                CategoryId = categoryId,
                IsPrimary = isPrimary
            }, ct);
        }

        foreach (var skillId in skillIds)
        {
            await _context.Set<TeamSkill>().AddAsync(new TeamSkill
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                SkillId = skillId
            }, ct);
        }
    }

    public async Task ReplaceCategoriesAsync(Guid teamId, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories, CancellationToken ct = default)
    {
        await _context.Set<TeamCategory>()
            .Where(tc => tc.TeamId == teamId)
            .IgnoreQueryFilters()
            .ExecuteDeleteAsync(ct);

        foreach (var (categoryId, isPrimary) in categories)
        {
            await _context.Set<TeamCategory>().AddAsync(new TeamCategory
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                CategoryId = categoryId,
                IsPrimary = isPrimary
            }, ct);
        }
    }

    public async Task ReplaceSkillsAsync(Guid teamId, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        await _context.Set<TeamSkill>()
            .Where(ts => ts.TeamId == teamId)
            .IgnoreQueryFilters()
            .ExecuteDeleteAsync(ct);

        foreach (var skillId in skillIds)
        {
            await _context.Set<TeamSkill>().AddAsync(new TeamSkill
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                SkillId = skillId
            }, ct);
        }
    }

    public async Task UpdateRatingAsync(Guid teamId, decimal averageRating, int ratingCount, CancellationToken ct = default)
    {
        await _dbSet
            .Where(t => t.Id == teamId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(t => t.AverageRating, averageRating)
                    .SetProperty(t => t.RatingCount, ratingCount),
                ct);
    }
    
    public Task<bool> TeamCodeExistsAsync(string teamCode, CancellationToken ct = default)
    {
        return _dbSet.AsNoTracking().AnyAsync(t => t.TeamCode == teamCode, ct);
    }

    public async Task ReplaceSpecialtiesAsync(Guid teamId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default)
    {
        await _context.Set<TeamSpecialty>()
            .Where(ts => ts.TeamId == teamId)
            .IgnoreQueryFilters()
            .ExecuteDeleteAsync(ct);

        foreach (var specialtyId in specialtyIds)
        {
            await _context.Set<TeamSpecialty>().AddAsync(new TeamSpecialty
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                SpecialtyId = specialtyId
            }, ct);
        }
    }


}
