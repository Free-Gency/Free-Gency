using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IProjectEventRepository : IGenericRepository<ProjectEvent>
{
    Task<IEnumerable<ProjectEvent>> GetByProjectIdAsync(Guid projectId,int skip,int take,CancellationToken ct = default);

    Task<ProjectEvent?> GetLatestByTypeAsync(Guid projectId,EventType eventType,CancellationToken ct = default);
}