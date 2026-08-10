

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IProjectMemberRepository : IGenericRepository<ProjectMember>
{
    Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId,CancellationToken ct = default);
    Task<bool> IsMemberAsync(Guid projectId,Guid userId,CancellationToken ct = default);
    Task RemoveAsync(Guid projectId,Guid userId,CancellationToken ct = default);
    Task AddSoloAssigneeAsync(Guid projectId,Guid userId,Guid assignedByUserId,CancellationToken ct = default);

    
    Task<IEnumerable<ProjectMember>> GetMembersWithUsersAsync(Guid projectId, CancellationToken ct = default);
    Task<IEnumerable<ProjectMember>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddMemberAsync(Guid projectId, Guid userId, string roleInProject, Guid assignedByUserId, CancellationToken ct = default);
}
