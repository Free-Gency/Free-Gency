namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class PortfolioRepository
    : GenericRepository<PortfolioProject>,
      IPortfolioRepository
    {
        public PortfolioRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public new IQueryable<PortfolioProject> Query()
        {
            return _dbSet
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.PortfolioImages)
                .Include(x => x.PortfolioSkills)
                    .ThenInclude(x => x.Skill);
        }

        public async Task<PortfolioProject?> GetDetailsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            return await Query()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<PortfolioProject>> GetDeveloperPortfolioAsync(
            Guid developerId,
            CancellationToken ct = default)
        {
            return await Query()
                .Where(x =>
                    x.OwnerUserId == developerId &&
                    x.Visibility == Visibility.Public)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<PortfolioProject>> GetTeamPortfolioAsync(
            Guid teamId,
            CancellationToken ct = default)
        {
            return await Query()
                .Where(x =>
                    x.OwnerTeamId == teamId &&
                    x.Visibility == Visibility.Public)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<bool> IsOwnerAsync(
            Guid portfolioProjectId,
            Guid userId,
            CancellationToken ct = default)
        {
            return await _dbSet.AnyAsync(x =>
                x.Id == portfolioProjectId &&
                x.OwnerUserId == userId,
                ct);
        }

        public async Task<bool> IsTeamOwnerAsync(
            Guid portfolioProjectId,
            Guid teamId,
            CancellationToken ct = default)
        {
            return await _dbSet.AnyAsync(x =>
                x.Id == portfolioProjectId &&
                x.OwnerTeamId == teamId,
                ct);
        }

        public async Task AddWithSkillsAsync(
            PortfolioProject project,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(project);

            if (project.Id == Guid.Empty)
                project.Id = Guid.NewGuid();

            await _dbSet.AddAsync(project, ct);

            var skills = skillIds.Distinct().ToList();

            if (!skills.Any())
                return;

            var entities = skills.Select(skillId =>
                new PortfolioSkill
                {
                    Id = Guid.NewGuid(),
                    PortfolioProjectId = project.Id,
                    SkillId = skillId
                });

            await _context
                .Set<PortfolioSkill>()
                .AddRangeAsync(entities, ct);
        }

        public async Task ReplaceSkillsAsync(
            Guid portfolioProjectId,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default)
        {
            if (skillIds.Count() > 0)
            {
                var oldSkills = await _context
                .Set<PortfolioSkill>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

                if (oldSkills.Count > 0)
                    _context.RemoveRange(oldSkills);

                var skills = skillIds.Distinct().ToList();

                if (!skills.Any())
                    return;

                var entities = skills.Select(skillId =>
                    new PortfolioSkill
                    {
                        Id = Guid.NewGuid(),
                        PortfolioProjectId = portfolioProjectId,
                        SkillId = skillId
                    });

                await _context
                    .Set<PortfolioSkill>()
                    .AddRangeAsync(entities, ct);
            }
        }

        public async Task AddImagesAsync(
            Guid portfolioProjectId,
            IEnumerable<string> imageUrls,
            CancellationToken ct = default)
        {
            var currentCount = await _context
                .Set<PortfolioImage>()
                .CountAsync(x =>
                    x.PortfolioProjectId == portfolioProjectId,
                    ct);

            var entities = imageUrls.Select((url, index) =>
                new PortfolioImage
                {
                    Id = Guid.NewGuid(),
                    PortfolioProjectId = portfolioProjectId,
                    ImageUrl = url,
                    SortOrder = currentCount + index
                });

            await _context
                .Set<PortfolioImage>()
                .AddRangeAsync(entities, ct);
        }

        public async Task DeleteImageAsync(
            Guid imageId,
            CancellationToken ct = default)
        {
            var image = await _context
                .Set<PortfolioImage>()
                .FirstOrDefaultAsync(x => x.Id == imageId, ct);

            if (image == null)
                return;

            _context.Remove(image);
        }


    }
}
