using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Teams;

public sealed class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TeamHubItem>> GetMyHubItemsAsync(Guid userId, CancellationToken ct = default)
    {
        var teams = await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(t => t.OwnerUserId == userId || t.TeamMembers.Any(tm => tm.UserId == userId))
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Include(t => t.Owner).ThenInclude(u => u!.DeveloperProfile)
            .Include(t => t.Owner).ThenInclude(u => u!.ClientProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.DeveloperProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.ClientProfile)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .ToListAsync(ct);

        var projectCounts = await GetProjectsCountsAsync(teams.Select(t => t.Id).ToList(), ct);
        return teams.Select(t => MapHubItem(t, userId, projectCounts)).ToList();
    }

    public async Task<IReadOnlyList<TeamHubItem>> GetBrowseHubItemsAsync(Guid? currentUserId, CancellationToken ct = default)
    {
        var teams = await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Include(t => t.Owner).ThenInclude(u => u!.DeveloperProfile)
            .Include(t => t.Owner).ThenInclude(u => u!.ClientProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.DeveloperProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.ClientProfile)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .ToListAsync(ct);

        var projectCounts = await GetProjectsCountsAsync(teams.Select(t => t.Id).ToList(), ct);
        return teams.Select(t => MapHubItem(t, currentUserId, projectCounts)).ToList();
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

        var teams = await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(t => pageIds.Contains(t.Id))
            .Include(t => t.Owner).ThenInclude(u => u!.DeveloperProfile)
            .Include(t => t.Owner).ThenInclude(u => u!.ClientProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.DeveloperProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.ClientProfile)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .ToListAsync(ct);

        var order = pageIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
        teams = teams.OrderBy(t => order.GetValueOrDefault(t.Id, int.MaxValue)).ToList();

        var projectCounts = await GetProjectsCountsAsync(pageIds, ct);
        var items = teams.Select(t => MapHubItem(t, currentUserId, projectCounts)).ToList();
        return (items, totalCount);
    }

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
            .AsSplitQuery()
            .Where(t => t.OwnerUserId == ownerUserId)
            .Include(t => t.Owner)
            .Include(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.DeveloperProfile)
            .Include(t => t.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.ClientProfile)
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

    private static TeamHubItem MapHubItem(
        Team t,
        Guid? currentUserId,
        IReadOnlyDictionary<Guid, int> projectCounts)
    {
        var membersCount = t.TeamMembers?.Count ?? 0;
        return new TeamHubItem
        {
            Id = t.Id,
            Name = t.Name,
            Logo = t.Logo,
            Cover = t.Cover,
            TeamCode = t.TeamCode,
            AboutUs = t.AboutUs,
            AverageRating = t.AverageRating,
            RatingCount = t.RatingCount,
            OwnerUserId = t.OwnerUserId,
            OwnerName = $"{t.Owner?.FristName ?? string.Empty} {t.Owner?.LastName ?? string.Empty}".Trim(),
            MembersCount = membersCount,
            ProjectsCount = projectCounts.GetValueOrDefault(t.Id),
            MyRole = ResolveMyRole(t, currentUserId),
            MemberAvatars = BuildMemberAvatars(t),
            Categories = (t.TeamCategories ?? [])
                .Select(tc => new TeamCategoryItem
                {
                    CategoryId = tc.CategoryId,
                    Name = tc.Category?.Name ?? string.Empty,
                    NameEn = tc.Category?.NameEn ?? string.Empty,
                    IsPrimary = tc.IsPrimary
                })
                .ToList(),
            Specialties = (t.TeamSpecialties ?? [])
                .Select(ts => new TeamSpecialtyItem
                {
                    SpecialtyId = ts.SpecialtyId,
                    NameEn = ts.Specialty?.NameEn ?? string.Empty,
                    NameAr = ts.Specialty?.NameAr ?? string.Empty
                })
                .ToList(),
            Skills = (t.TeamSkills ?? [])
                .Select(ts => new TeamSkillItem
                {
                    SkillId = ts.SkillId,
                    Name = ts.Skill?.Name ?? string.Empty
                })
                .ToList()
        };
    }

    private static string? ResolveMyRole(Team t, Guid? currentUserId)
    {
        if (currentUserId is null || currentUserId == Guid.Empty)
            return null;

        if (t.OwnerUserId == currentUserId)
            return nameof(Role.TeamLeader);

        var membership = t.TeamMembers?.FirstOrDefault(tm => tm.UserId == currentUserId);
        return membership is null ? null : membership.TeamRole.ToString();
    }

    private static List<TeamMemberAvatarItem> BuildMemberAvatars(Team t)
    {
        var fromMembers = (t.TeamMembers ?? [])
            .OrderBy(tm => tm.TeamRole == Role.TeamLeader ? 0 : 1)
            .ThenBy(tm => tm.JoinedAt)
            .Select(tm => new TeamMemberAvatarItem
            {
                UserId = tm.UserId,
                Name = $"{tm.User?.FristName ?? string.Empty} {tm.User?.LastName ?? string.Empty}".Trim(),
                ImageUrl = tm.User?.DeveloperProfile?.ProfileImage
                    ?? tm.User?.ClientProfile?.ProfileImage
            })
            .Where(a => a.UserId != Guid.Empty)
            .ToList();

        if (fromMembers.Count > 0)
            return fromMembers;

        var ownerName = $"{t.Owner?.FristName ?? string.Empty} {t.Owner?.LastName ?? string.Empty}".Trim();
        if (t.OwnerUserId == Guid.Empty)
            return [];

        return
        [
            new TeamMemberAvatarItem
            {
                UserId = t.OwnerUserId,
                Name = string.IsNullOrWhiteSpace(ownerName) ? "Owner" : ownerName,
                ImageUrl = t.Owner?.DeveloperProfile?.ProfileImage
                    ?? t.Owner?.ClientProfile?.ProfileImage
            }
        ];
    }
}
