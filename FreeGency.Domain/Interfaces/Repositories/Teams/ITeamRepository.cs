
using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories.Teams;

public interface ITeamRepository : IGenericRepository<Team>
{
    Task<IReadOnlyList<TeamHubItem>> GetMyHubItemsAsync(Guid userId, CancellationToken ct = default);

    Task<(IReadOnlyList<TeamHubItem> Items, int TotalCount)> GetBrowseHubItemsPagedAsync(
        Guid? currentUserId,
        string? search,
        Guid? categoryId,
        bool excludeMine,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<Team?> GetByTeamCodeWithDetailsAsync(string teamCode, CancellationToken ct = default);

    Task<Team?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);


    Task AddWithTaxonomyAsync(Team team, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories,
                            IEnumerable<Guid> skillIds, CancellationToken ct = default);


    Task ReplaceCategoriesAsync(Guid teamId, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories, CancellationToken ct = default);

    Task ReplaceSkillsAsync(Guid teamId, IEnumerable<Guid> skillIds, CancellationToken ct = default);

    Task UpdateRatingAsync(Guid teamId, decimal averageRating, int ratingCount, CancellationToken ct = default);

    Task<IReadOnlyList<TeamFeedback>> GetFeedbackAsync(Guid teamId, int take, CancellationToken ct = default);

    Task<bool> HasFeedbackAsync(Guid teamId, Guid reviewerUserId, CancellationToken ct = default);

    Task AddFeedbackAsync(TeamFeedback feedback, CancellationToken ct = default);

    Task<bool> TeamCodeExistsAsync(string teamCode, CancellationToken ct = default);
    
    Task ReplaceSpecialtiesAsync(Guid teamId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default);
}

public sealed class TeamHubItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Cover { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string? AboutUs { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int MembersCount { get; set; }
    public int ProjectsCount { get; set; }
    public string? MyRole { get; set; }
    public List<TeamMemberAvatarItem> MemberAvatars { get; set; } = [];
    public List<TeamCategoryItem> Categories { get; set; } = [];
    public List<TeamSpecialtyItem> Specialties { get; set; } = [];
    public List<TeamSkillItem> Skills { get; set; } = [];
}

public sealed class TeamMemberAvatarItem
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public sealed class TeamCategoryItem
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public sealed class TeamSpecialtyItem
{
    public Guid SpecialtyId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}

public sealed class TeamSkillItem
{
    public Guid SkillId { get; set; }
    public string Name { get; set; } = string.Empty;
}
