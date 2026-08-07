
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories.Teams;

public interface ITeamMemberRepository : IGenericRepository<TeamMember>
{
    Task<List<Guid>> GetLeaderDeveloperProfileIdsAsync(Guid teamId);
    Task<IReadOnlyList<TeamMember>> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default);

    Task<IReadOnlyList<TeamMember>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<TeamMember?> GetSingleInTeamAsync(Guid teamId, Guid userId, CancellationToken ct = default);

    Task<TeamMember?> GetTrackedSingleInTeamAsync(Guid teamId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<TeamMember>> GetByTeamIdWithUserAsync(Guid teamId, CancellationToken ct = default);
    
    Task<int> GetMemberCountAsync(Guid teamId, CancellationToken ct = default);
    
    
    Task<bool> HasRoleAsync(Guid teamId, Guid userId, Role role, CancellationToken ct = default);

    Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default);
    
    Task<bool> IsLeaderAsync(Guid teamId, Guid userId, CancellationToken ct = default);
    
    Task<IReadOnlyList<TeamMember>> GetLeadersAsync(Guid teamId, CancellationToken ct = default);
    
    Task RemoveAsync(Guid teamId, Guid userId, CancellationToken ct = default);

    Task RemoveByTeamIdAsync(Guid teamId, CancellationToken ct = default);
}


