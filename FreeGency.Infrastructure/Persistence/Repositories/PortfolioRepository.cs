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
                .Include(x => x.OwnerUser)
                .Include(x => x.OwnerTeam)
                .Include(x => x.PortfolioImages)
                .Include(x => x.PortfolioSkills)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.RoadmapSteps)
                .Include(x => x.Metrics);
        }

        public IQueryable<PortfolioProject> GetInspirationQuery(
            Guid? categoryId,
            string? search)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.OwnerUser)
                .Include(x => x.OwnerTeam)
                .Where(x => x.Visibility == Visibility.Public);

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.Title.Contains(term) ||
                    x.Description.Contains(term));
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task RecordViewAsync(
            Guid userId,
            Guid portfolioProjectId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var set = _context.Set<RecentlyViewedPortfolio>();

            var existing = await set
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.PortfolioProjectId == portfolioProjectId,
                    ct);

            if (existing is null)
            {
                await set.AddAsync(new RecentlyViewedPortfolio
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PortfolioProjectId = portfolioProjectId,
                    ViewedAt = now,
                    CreatedAt = now,
                    CreatedBy = userId.ToString(),
                }, ct);
            }
            else
            {
                existing.ViewedAt = now;
                existing.UpdatedAt = now;
                existing.UpdatedBy = userId.ToString();
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.DeletedBy = null;
            }

            var overflow = await set
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.ViewedAt)
                .Skip(20)
                .ToListAsync(ct);

            if (overflow.Count > 0)
                _context.RemoveRange(overflow);
        }

        public async Task<IReadOnlyList<(PortfolioProject Project, DateTime ViewedAt)>> GetRecentlyViewedByUserAsync(
            Guid userId,
            int take,
            CancellationToken ct = default)
        {
            take = Math.Clamp(take <= 0 ? 5 : take, 1, 20);

            var rows = await _context.Set<RecentlyViewedPortfolio>()
                .AsNoTracking()
                .Include(x => x.PortfolioProject)
                    .ThenInclude(p => p.Category)
                .Include(x => x.PortfolioProject)
                    .ThenInclude(p => p.OwnerUser)
                .Include(x => x.PortfolioProject)
                    .ThenInclude(p => p.OwnerTeam)
                .Where(x =>
                    x.UserId == userId &&
                    x.PortfolioProject.Visibility == Visibility.Public)
                .OrderByDescending(x => x.ViewedAt)
                .Take(take)
                .ToListAsync(ct);

            return rows
                .Select(x => (x.PortfolioProject, x.ViewedAt))
                .ToList();
        }

        public async Task<IReadOnlyList<PortfolioFeedback>> GetFeedbackAsync(
            Guid portfolioProjectId,
            int take,
            CancellationToken ct = default)
        {
            take = Math.Clamp(take <= 0 ? 20 : take, 1, 50);

            return await _context.Set<PortfolioFeedback>()
                .AsNoTracking()
                .Include(x => x.ReviewerUser!)
                    .ThenInclude(u => u.ClientProfile)
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<bool> HasFeedbackAsync(
            Guid portfolioProjectId,
            Guid reviewerUserId,
            CancellationToken ct = default)
            => _context.Set<PortfolioFeedback>().AnyAsync(
                x => x.PortfolioProjectId == portfolioProjectId &&
                     x.ReviewerUserId == reviewerUserId,
                ct);

        public Task AddFeedbackAsync(
            PortfolioFeedback feedback,
            CancellationToken ct = default)
            => _context.Set<PortfolioFeedback>().AddAsync(feedback, ct).AsTask();

        public async Task<PortfolioProject?> GetDetailsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            return await Query()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<PortfolioProject?> GetPublicDetailsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Category)
                .Include(x => x.PortfolioImages)
                .Include(x => x.PortfolioSkills)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.RoadmapSteps)
                .Include(x => x.Metrics)
                .Include(x => x.OwnerUser!)
                    .ThenInclude(u => u.DeveloperProfile)
                .Include(x => x.OwnerTeam!)
                    .ThenInclude(t => t.TeamMembers)
                .Include(x => x.OwnerTeam!)
                    .ThenInclude(t => t.TeamSpecialties!)
                        .ThenInclude(s => s.Specialty)
                .Include(x => x.OwnerTeam!)
                    .ThenInclude(t => t.TeamSkills!)
                        .ThenInclude(s => s.Skill)
                .FirstOrDefaultAsync(
                    x => x.Id == id && x.Visibility == Visibility.Public,
                    ct);
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

        public Task<PortfolioImage?> GetImageByIdAsync(
            Guid imageId,
            CancellationToken ct = default)
            => _context
                .Set<PortfolioImage>()
                .FirstOrDefaultAsync(x => x.Id == imageId, ct);

        public async Task DeleteImagesAndSkillsAsync(
            Guid portfolioProjectId,
            CancellationToken ct = default)
        {
            var images = await _context
                .Set<PortfolioImage>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

            if (images.Count > 0)
                _context.RemoveRange(images);

            var skills = await _context
                .Set<PortfolioSkill>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

            if (skills.Count > 0)
                _context.RemoveRange(skills);

            var steps = await _context
                .Set<PortfolioRoadmapStep>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

            if (steps.Count > 0)
                _context.RemoveRange(steps);

            var metrics = await _context
                .Set<PortfolioMetric>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

            if (metrics.Count > 0)
                _context.RemoveRange(metrics);
        }

        public async Task ReplaceRoadmapStepsAsync(
            Guid portfolioProjectId,
            IEnumerable<PortfolioRoadmapStep> steps,
            CancellationToken ct = default)
        {
            var old = await _context
                .Set<PortfolioRoadmapStep>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

            if (old.Count > 0)
                _context.RemoveRange(old);

            var list = steps?.ToList() ?? [];
            if (list.Count == 0)
                return;

            foreach (var step in list)
            {
                if (step.Id == Guid.Empty)
                    step.Id = Guid.NewGuid();
                step.PortfolioProjectId = portfolioProjectId;
            }

            await _context.Set<PortfolioRoadmapStep>().AddRangeAsync(list, ct);
        }

        public async Task ReplaceMetricsAsync(
            Guid portfolioProjectId,
            IEnumerable<PortfolioMetric> metrics,
            CancellationToken ct = default)
        {
            var old = await _context
                .Set<PortfolioMetric>()
                .Where(x => x.PortfolioProjectId == portfolioProjectId)
                .ToListAsync(ct);

            if (old.Count > 0)
                _context.RemoveRange(old);

            var list = metrics?.ToList() ?? [];
            if (list.Count == 0)
                return;

            foreach (var metric in list)
            {
                if (metric.Id == Guid.Empty)
                    metric.Id = Guid.NewGuid();
                metric.PortfolioProjectId = portfolioProjectId;
            }

            await _context.Set<PortfolioMetric>().AddRangeAsync(list, ct);
        }
    }
}
