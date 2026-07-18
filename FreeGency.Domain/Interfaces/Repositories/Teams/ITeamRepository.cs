
using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories.Teams;

public interface ITeamRepository : IGenericRepository<Team>
{
    Task<IReadOnlyList<Team>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default);

    Task<IReadOnlyList<Team>> GetByOwnerUserIdWithDetailsAsync(Guid ownerUserId, CancellationToken ct = default);

    Task<Team?> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default);

    Task<Team?> GetByTeamCodeWithDetailsAsync(string teamCode, CancellationToken ct = default);

    Task<Team?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);


    Task AddWithTaxonomyAsync(Team team, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories,
                            IEnumerable<Guid> skillIds, CancellationToken ct = default);


    Task ReplaceCategoriesAsync(Guid teamId, IEnumerable<(Guid CategoryId, bool IsPrimary)> categories, CancellationToken ct = default);

    Task ReplaceSkillsAsync(Guid teamId, IEnumerable<Guid> skillIds, CancellationToken ct = default);

    Task UpdateRatingAsync(Guid teamId, decimal averageRating, int ratingCount, CancellationToken ct = default);


    Task<bool> TeamCodeExistsAsync(string teamCode, CancellationToken ct = default);
}
