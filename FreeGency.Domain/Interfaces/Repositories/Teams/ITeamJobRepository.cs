using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories.Teams;

public interface ITeamJobRepository : IGenericRepository<TeamJob>
{
    Task<IReadOnlyList<TeamJob>> GetOpenByTeamIdAsync(Guid teamId, CancellationToken ct = default);

    Task<IReadOnlyList<TeamJob>> GetOpenJobsAsync(
        Guid? teamId = null,
        IEnumerable<Guid>? skillIds = null,
        int skip = 0,
        int take = 20,
        CancellationToken ct = default);

    Task<TeamJob?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TeamJob>> GetAllByTeamIdAsync(Guid teamId, CancellationToken ct = default);


    Task AddWithSkillsAsync(TeamJob job, IEnumerable<Guid> skillIds, CancellationToken ct = default);
    
    Task CloseAsync(Guid id, DateTime closedAt, CancellationToken ct = default);
}
