using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Teams;

public sealed class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDbContext context) : base(context) { }

    private const int HubAvatarLimit = 3;
    public IQueryable<Project> GetProjectTeamAccepted(Guid TeamId)
    {
        return _context.Projects
       .Where(x =>
           x.AssignedTeamId == TeamId &&
           x.MilestonePlanVersions.Any(v =>
               v.Status == PlanVersionStatus.Accepted));
    }
    public async Task<IReadOnlyList<TeamHubItem>> GetMyHubItemsAsync(Guid userId, CancellationToken ct = default)
    {
        var teamIds = await _dbSet
            .AsNoTracking()
            .Where(t => t.OwnerUserId == userId || t.TeamMembers.Any(tm => tm.UserId == userId))
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync(ct);

        return await LoadHubItemsByIdsAsync(teamIds, userId, ct);
    }

    public async Task<(IReadOnlyList<TeamHubItem> Items, int TotalCount)> GetBrowseHubItemsPagedAsync(
        Guid? currentUserId,
        string? search,
        Guid? categoryId,
        bool excludeMine,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (excludeMine && currentUserId is Guid uid && uid != Guid.Empty)
        {
            query = query.Where(t =>
                t.OwnerUserId != uid &&
                !t.TeamMembers.Any(tm => tm.UserId == uid));
        }

        if (categoryId is Guid catId && catId != Guid.Empty)
        {
            query = query.Where(t => t.TeamCategories.Any(tc => tc.CategoryId == catId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.Name.Contains(term) ||
                (t.AboutUs != null && t.AboutUs.Contains(term)) ||
                (t.Owner != null && (
                    (t.Owner.FristName != null && t.Owner.FristName.Contains(term)) ||
                    (t.Owner.LastName != null && t.Owner.LastName.Contains(term)))) ||
                t.TeamSpecialties.Any(ts =>
                    (ts.Specialty != null && ts.Specialty.NameEn != null && ts.Specialty.NameEn.Contains(term)) ||
                    (ts.Specialty != null && ts.Specialty.NameAr != null && ts.Specialty.NameAr.Contains(term))) ||
                t.TeamSkills.Any(ts =>
                    ts.Skill != null && ts.Skill.Name != null && ts.Skill.Name.Contains(term)));
        }

        query = query.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
            return ([], 0);

        var pageIds = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync(ct);

        if (pageIds.Count == 0)
            return ([], totalCount);

        var items = await LoadHubItemsByIdsAsync(pageIds, currentUserId, ct);
        return (items, totalCount);
    }

    /// <summary>
    /// Lightweight hub payload: projected columns only (no full member/profile graphs).
    /// </summary>
    private async Task<IReadOnlyList<TeamHubItem>> LoadHubItemsByIdsAsync(
        IReadOnlyList<Guid> teamIds,
        Guid? currentUserId,
        CancellationToken ct)
    {
        if (teamIds.Count == 0)
            return [];

        var cores = await _dbSet
            .AsNoTracking()
            .Where(t => teamIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Logo,
                t.Cover,
                t.TeamCode,
                t.AboutUs,
                t.AverageRating,
                t.RatingCount,
                t.OwnerUserId,
                OwnerFirstName = t.Owner != null ? t.Owner.FristName : null,
                OwnerLastName = t.Owner != null ? t.Owner.LastName : null,
                OwnerDevImage = t.Owner != null && t.Owner.DeveloperProfile != null
                    ? t.Owner.DeveloperProfile.ProfileImage
                    : null,
                OwnerClientImage = t.Owner != null && t.Owner.ClientProfile != null
                    ? t.Owner.ClientProfile.ProfileImage
                    : null,
                MembersCount = t.TeamMembers.Count(),
                MyMemberRole = currentUserId != null && currentUserId != Guid.Empty
                    ? t.TeamMembers
                        .Where(tm => tm.UserId == currentUserId)
                        .Select(tm => (Role?)tm.TeamRole)
                        .FirstOrDefault()
                    : null,
            })
            .ToListAsync(ct);

        var categoryRows = await _context.Set<TeamCategory>()
            .AsNoTracking()
            .Where(tc => teamIds.Contains(tc.TeamId))
            .Select(tc => new
            {
                tc.TeamId,
                tc.CategoryId,
                Name = tc.Category != null ? tc.Category.Name ?? string.Empty : string.Empty,
                NameEn = tc.Category != null ? tc.Category.NameEn ?? string.Empty : string.Empty,
                tc.IsPrimary,
            })
            .ToListAsync(ct);

        var skillRows = await _context.Set<TeamSkill>()
            .AsNoTracking()
            .Where(ts => teamIds.Contains(ts.TeamId))
            .Select(ts => new
            {
                ts.TeamId,
                ts.SkillId,
                Name = ts.Skill != null ? ts.Skill.Name ?? string.Empty : string.Empty,
            })
            .ToListAsync(ct);

        // Project only avatar fields — then keep top 3 per team in memory.
        var memberRows = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId))
            .Select(tm => new
            {
                tm.TeamId,
                tm.UserId,
                tm.TeamRole,
                tm.JoinedAt,
                FirstName = tm.User != null ? tm.User.FristName : null,
                LastName = tm.User != null ? tm.User.LastName : null,
                DevImage = tm.User != null && tm.User.DeveloperProfile != null
                    ? tm.User.DeveloperProfile.ProfileImage
                    : null,
                ClientImage = tm.User != null && tm.User.ClientProfile != null
                    ? tm.User.ClientProfile.ProfileImage
                    : null,
            })
            .ToListAsync(ct);

        var avatarsByTeam = memberRows
            .GroupBy(m => m.TeamId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(m => m.TeamRole == Role.TeamLeader ? 0 : 1)
                    .ThenBy(m => m.JoinedAt)
                    .Take(HubAvatarLimit)
                    .Select(m => new TeamMemberAvatarItem
                    {
                        UserId = m.UserId,
                        Name = $"{m.FirstName ?? string.Empty} {m.LastName ?? string.Empty}".Trim(),
                        ImageUrl = m.DevImage ?? m.ClientImage,
                    })
                    .ToList());

        var categoriesByTeam = categoryRows
            .GroupBy(c => c.TeamId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new TeamCategoryItem
                {
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    NameEn = x.NameEn,
                    IsPrimary = x.IsPrimary,
                }).ToList());

        var skillsByTeam = skillRows
            .GroupBy(s => s.TeamId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new TeamSkillItem
                {
                    SkillId = x.SkillId,
                    Name = x.Name,
                }).ToList());

        var projectCounts = await GetProjectsCountsAsync(teamIds, ct);
        var order = teamIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);

        return cores
            .OrderBy(c => order.GetValueOrDefault(c.Id, int.MaxValue))
            .Select(c =>
            {
                string? myRole = null;
                if (currentUserId is Guid uid && uid != Guid.Empty)
                {
                    if (c.OwnerUserId == uid)
                        myRole = nameof(Role.TeamLeader);
                    else if (c.MyMemberRole is Role role)
                        myRole = role.ToString();
                }

                var ownerName = $"{c.OwnerFirstName ?? string.Empty} {c.OwnerLastName ?? string.Empty}".Trim();
                var avatars = avatarsByTeam.GetValueOrDefault(c.Id) ?? [];
                if (avatars.Count == 0 && c.OwnerUserId != Guid.Empty)
                {
                    avatars =
                    [
                        new TeamMemberAvatarItem
                        {
                            UserId = c.OwnerUserId,
                            Name = string.IsNullOrWhiteSpace(ownerName) ? "Owner" : ownerName,
                            ImageUrl = c.OwnerDevImage ?? c.OwnerClientImage,
                        },
                    ];
                }

                return new TeamHubItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Logo = c.Logo,
                    Cover = c.Cover,
                    TeamCode = c.TeamCode,
                    AboutUs = TruncateHubAbout(c.AboutUs),
                    AverageRating = c.AverageRating,
                    RatingCount = c.RatingCount,
                    OwnerUserId = c.OwnerUserId,
                    OwnerName = ownerName,
                    MembersCount = c.MembersCount,
                    ProjectsCount = projectCounts.GetValueOrDefault(c.Id),
                    MyRole = myRole,
                    MemberAvatars = avatars,
                    Categories = categoriesByTeam.GetValueOrDefault(c.Id) ?? [],
                    // Hub cards don't render specialties — skip the join entirely.
                    Specialties = [],
                    Skills = skillsByTeam.GetValueOrDefault(c.Id) ?? [],
                };
            })
            .ToList();
    }

    private static string? TruncateHubAbout(string? about)
    {
        if (string.IsNullOrWhiteSpace(about))
            return about;

        const int max = 220;
        var trimmed = about.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max].TrimEnd() + "…";
    }

    public async Task<Team?> GetByTeamCodeWithDetailsAsync(string teamCode, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Owner)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.DeveloperProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.ClientProfile)
            .Include(t => t.SocialLinks)
            .FirstOrDefaultAsync(t => t.TeamCode == teamCode, ct);
    }

    public async Task<Team?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Owner)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.DeveloperProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.ClientProfile)
            .Include(t => t.SocialLinks)
            .Include(t => t.AssignedProjects)
            .Include(t => t.PortfolioProjects)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddWithTaxonomyAsync(Team team, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(team, ct);

        var categoryList = categories
            .GroupBy(c => c.CategoryId)
            .Select(g => g.First())
            .ToList();

        if (categoryList.Count > 0 && categoryList.All(c => !c.IsPrimary))
        {
            var first = categoryList[0];
            categoryList[0] = (first.CategoryId, true);
        }

        foreach (var (categoryId, isPrimary) in categoryList)
        {
            await _context.Set<TeamCategory>().AddAsync(new TeamCategory
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                CategoryId = categoryId,
                IsPrimary = isPrimary
            }, ct);
        }

        foreach (var skillId in skillIds.Distinct())
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

        var categoryList = categories
            .GroupBy(c => c.CategoryId)
            .Select(g => g.First())
            .ToList();

        if (categoryList.Count > 0 && categoryList.All(c => !c.IsPrimary))
        {
            var first = categoryList[0];
            categoryList[0] = (first.CategoryId, true);
        }

        foreach (var (categoryId, isPrimary) in categoryList)
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

    public async Task<IReadOnlyList<TeamFeedback>> GetFeedbackAsync(Guid teamId, int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take <= 0 ? 20 : take, 1, 50);

        return await _context.Set<TeamFeedback>()
            .AsNoTracking()
            .Include(x => x.ReviewerUser!)
                .ThenInclude(u => u.ClientProfile)
            .Include(x => x.ReviewerUser!)
                .ThenInclude(u => u.DeveloperProfile)
            .Where(x => x.TeamId == teamId
                        && x.ModerationStatus != FreeGency.Domain.Enums.ModerationStatus.Hidden)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<bool> HasFeedbackAsync(Guid teamId, Guid reviewerUserId, CancellationToken ct = default)
        => _context.Set<TeamFeedback>().AnyAsync(
            x => x.TeamId == teamId && x.ReviewerUserId == reviewerUserId,
            ct);

    public Task AddFeedbackAsync(TeamFeedback feedback, CancellationToken ct = default)
        => _context.Set<TeamFeedback>().AddAsync(feedback, ct).AsTask();

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

    private async Task<Dictionary<Guid, int>> GetProjectsCountsAsync(
        IReadOnlyList<Guid> teamIds,
        CancellationToken ct)
    {
        if (teamIds.Count == 0)
            return new Dictionary<Guid, int>();

        var portfolioCounts = await _context.Set<PortfolioProject>()
            .AsNoTracking()
            .Where(p => p.OwnerTeamId != null && teamIds.Contains(p.OwnerTeamId.Value))
            .GroupBy(p => p.OwnerTeamId!.Value)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var completedCounts = await _context.Set<Project>()
            .AsNoTracking()
            .Where(p =>
                p.AssignedTeamId != null
                && teamIds.Contains(p.AssignedTeamId.Value)
                && p.Status == ProjectStatus.Completed)
            .GroupBy(p => p.AssignedTeamId!.Value)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var result = teamIds.ToDictionary(id => id, _ => 0);
        foreach (var row in portfolioCounts)
            result[row.TeamId] = row.Count;
        foreach (var row in completedCounts)
            result[row.TeamId] = result.GetValueOrDefault(row.TeamId) + row.Count;

        return result;
    }

   
}
