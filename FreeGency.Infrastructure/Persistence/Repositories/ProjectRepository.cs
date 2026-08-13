namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context) { }



    public async Task<Project?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Id == id)
            .Include(p => p.Client)
                .ThenInclude(c => c.ClientProfile)
            .Include(p => p.Category)
            .Include(p => p.ProjectSpecialties)
                .ThenInclude(ps => ps.Specialty)
            .Include(p => p.ProjectSkills)
                .ThenInclude(ps => ps.Skill)
            .Include(p => p.ProjectProposals)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Project>> GetByClientIdAsync(Guid clientId, ProjectStatus? status = null, CancellationToken ct = default)
    {
        IQueryable<Project> query = _dbSet.AsNoTracking().Where(p => p.ClientId == clientId);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }
    public async Task<IEnumerable<Project>> SearchOpenAsync(string? keyword, Guid? categoryId, Guid? specialtyId, decimal? minBudget, decimal? maxBudget, CancellationToken ct = default)
    {
        IQueryable<Project> query = _dbSet.AsNoTracking().Where(p => p.Status == ProjectStatus.Open);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Title, $"%{keyword}%") ||
                EF.Functions.Like(p.Description, $"%{keyword}%"));
        }
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        if (specialtyId.HasValue)
            query = query.Where(p => p.ProjectSpecialties.Any(ps => ps.SpecialtyId == specialtyId.Value));
        if (minBudget.HasValue)
            query = query.Where(p => p.BudgetMax >= minBudget.Value);
        if (maxBudget.HasValue)
            query = query.Where(p => p.BudgetMin <= maxBudget.Value);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }
    public async Task AddWithSkillsAsync(Project project, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (project.Id == Guid.Empty)
            project.Id = Guid.NewGuid();

        await _dbSet.AddAsync(project, ct);

        var skills = skillIds.Distinct().ToList();

        if (!skills.Any())
            return;

        var projectSkills = skills.Select(skillId => new ProjectSkill
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            SkillId = skillId
        });

        await _context.Set<ProjectSkill>().AddRangeAsync(projectSkills, ct);
    }
    public async Task<IReadOnlyList<Guid>> GetInProgressIdsReadyToCompleteAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Status == ProjectStatus.InProgress)
            .Where(p => p.Milestones.Any() && p.Milestones.All(m => m.ReleaseStatus == ReleaseStatus.Released))
            .Select(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid id, ProjectStatus status, CancellationToken ct = default)
    {
        var project = await _dbSet.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
            throw new KeyNotFoundException("Project not found.");
        project.Status = status;
        if (status == ProjectStatus.Completed)
        {
            project.CompletedAt = DateTime.UtcNow;
        }
        _dbSet.Update(project);
    }
    public async Task SetAssigneeAsync(Guid id, Guid? userId, Guid? teamId, CancellationToken ct = default)
    {
        var project = await _dbSet.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
            throw new KeyNotFoundException("Project not found.");
        project.AssignedUserId = userId;
        project.AssignedTeamId = teamId;
        project.Status = ProjectStatus.InProgress;
        _dbSet.Update(project);
    }
    public async Task ReplaceSkillsAsync(Guid projectId, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        var desiredSkillIds = skillIds.Distinct().ToList();

        var existingLinks = await _context.Set<ProjectSkill>()
            .IgnoreQueryFilters()
            .Where(ps => ps.ProjectId == projectId)
            .ToListAsync(ct);

        var toRemove = existingLinks.Where(ps => !ps.IsDeleted && !desiredSkillIds.Contains(ps.SkillId));
        _context.Set<ProjectSkill>().RemoveRange(toRemove);

        foreach (var skillId in desiredSkillIds)
        {
            var existing = existingLinks.FirstOrDefault(ps => ps.SkillId == skillId);

            if (existing is null)
            {
                await _context.Set<ProjectSkill>().AddAsync(new ProjectSkill
                {
                    ProjectId = projectId,
                    SkillId = skillId,
                }, ct);
            }
            else if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.DeletedBy = null;
                _context.Set<ProjectSkill>().Update(existing);
            }
            // else: already active and still wanted — leave as is.
        }
    }

    public async Task ReplaceSpecialtiesAsync(Guid projectId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default)
    {
        var desiredSpecialtyIds = specialtyIds.Distinct().ToList();

        var existingLinks = await _context.Set<ProjectSpecialty>()
            .IgnoreQueryFilters()
            .Where(ps => ps.ProjectId == projectId)
            .ToListAsync(ct);

        var toRemove = existingLinks.Where(ps => !ps.IsDeleted && !desiredSpecialtyIds.Contains(ps.SpecialtyId));
        _context.Set<ProjectSpecialty>().RemoveRange(toRemove);

        foreach (var specialtyId in desiredSpecialtyIds)
        {
            var existing = existingLinks.FirstOrDefault(ps => ps.SpecialtyId == specialtyId);

            if (existing is null)
            {
                await _context.Set<ProjectSpecialty>().AddAsync(new ProjectSpecialty
                {
                    ProjectId = projectId,
                    SpecialtyId = specialtyId,
                }, ct);
            }
            else if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.DeletedBy = null;
                _context.Set<ProjectSpecialty>().Update(existing);
            }
        }
    }


    public async Task SaveProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var exists = await _context.Set<SavedProject>()
            .AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.UserId == userId, ct);

        if (exists) return;

        await _context.Set<SavedProject>().AddAsync(new SavedProject
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId
        }, ct);
    }
    public async Task UnsaveProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var savedProject = await _context.Set<SavedProject>().FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId, ct);
        if (savedProject is null)
            return;
        _context.Set<SavedProject>().Remove(savedProject);
    }
    public async Task<IEnumerable<Project>> GetSavedByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Set<SavedProject>().AsNoTracking().Where(x => x.UserId == userId).Select(x => x.Project).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    }

    public IQueryable<Project> GetProjectsQuery()
        => _dbSet.AsNoTracking();

    public async Task<IEnumerable<Project>> GetMineAsync(Guid userId, bool asClient, CancellationToken ct = default)
    {
        IQueryable<Project> query = _dbSet.AsNoTracking();

        query = asClient
            ? query.Where(p => p.ClientId == userId)
            : query.Where(p => p.AssignedUserId == userId || p.AssignedTeamId == userId);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }
}
