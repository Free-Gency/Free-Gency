

using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IProjectFileRepository : IGenericRepository<ProjectFile>
{
    Task<IEnumerable<ProjectFile>> GetByProjectIdAsync(Guid projectId,FileKind? kind = null,CancellationToken ct = default);
    Task<IEnumerable<ProjectFile>> GetByMilestoneIdAsync(Guid milestoneId,CancellationToken ct = default);
}
