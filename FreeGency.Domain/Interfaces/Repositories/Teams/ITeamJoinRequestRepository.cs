using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories.Teams;

public interface ITeamJoinRequestRepository : IGenericRepository<TeamJoinRequest>
{
    Task<IReadOnlyList<TeamJoinRequest>> GetPendingByTeamIdAsync(Guid teamId, CancellationToken ct = default);

    Task<IReadOnlyList<TeamJoinRequest>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<TeamJoinRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<bool> HasPendingRequestAsync(Guid teamId, Guid userId, CancellationToken ct = default);

    Task UpdateStatusAsync(Guid id, TeamJoinRequestStatus status, DateTime responseAt, string respondedByUserId, CancellationToken ct = default);
}
