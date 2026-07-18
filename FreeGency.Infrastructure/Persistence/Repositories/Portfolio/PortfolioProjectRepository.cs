
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Portfolio;

public sealed class PortfolioProjectRepository : GenericRepository<PortfolioProject>, IPortfolioProjectRepository
{
    public PortfolioProjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PortfolioProject>> GetByOwnerAsync(owner ownerType, Guid ownerId, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        query = ownerType switch
        {
            owner.User => query.Where(p => p.OwnerType == owner.User && p.OwnerUserId == ownerId),
            owner.Team => query.Where(p => p.OwnerType == owner.Team && p.OwnerTeamId == ownerId),
            _ => query.Where(_ => false)
        };

        return await query
            .Include(p => p.PortfolioImages)
            .Include(p => p.PortfolioSkills)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<PortfolioProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.PortfolioImages)
            .Include(p => p.PortfolioSkills).ThenInclude(ps => ps.Skill)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }



    public async Task AddWithDetailsAsync(PortfolioProject project, IEnumerable<(string ImageUrl, int SortOrder)> images, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(project, ct);

        foreach (var (imageUrl, sortOrder) in images)
        {
            await _context.Set<PortfolioImage>().AddAsync(new PortfolioImage
            {
                Id = Guid.NewGuid(),
                PortfolioProjectId = project.Id,
                ImageUrl = imageUrl,
                SortOrder = sortOrder
            }, ct);
        }

        foreach (var skillId in skillIds)
        {
            await _context.Set<PortfolioSkill>().AddAsync(new PortfolioSkill
            {
                Id = Guid.NewGuid(),
                PortfolioProjectId = project.Id,
                SkillId = skillId
            }, ct);
        }
    }

    public async Task ReplaceImagesAsync(
        Guid portfolioProjectId,
        IEnumerable<(string ImageUrl, int SortOrder)> images,
        CancellationToken ct = default)
    {
        await _context.Set<PortfolioImage>()
            .Where(i => i.PortfolioProjectId == portfolioProjectId)
            .IgnoreQueryFilters()
            .ExecuteDeleteAsync(ct);

        foreach (var (imageUrl, sortOrder) in images)
        {
            await _context.Set<PortfolioImage>().AddAsync(new PortfolioImage
            {
                Id = Guid.NewGuid(),
                PortfolioProjectId = portfolioProjectId,
                ImageUrl = imageUrl,
                SortOrder = sortOrder
            }, ct);
        }
    }

    public async Task ReplaceSkillsAsync(
        Guid portfolioProjectId,
        IEnumerable<Guid> skillIds,
        CancellationToken ct = default)
    {
        await _context.Set<PortfolioSkill>()
            .Where(s => s.PortfolioProjectId == portfolioProjectId)
            .IgnoreQueryFilters()
            .ExecuteDeleteAsync(ct);

        foreach (var skillId in skillIds)
        {
            await _context.Set<PortfolioSkill>().AddAsync(new PortfolioSkill
            {
                Id = Guid.NewGuid(),
                PortfolioProjectId = portfolioProjectId,
                SkillId = skillId
            }, ct);
        }
    }
}
