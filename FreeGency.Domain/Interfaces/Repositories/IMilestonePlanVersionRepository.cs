using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IMilestonePlanVersionRepository : IGenericRepository<MilestonePlanVersion>
{
    Task<IEnumerable<MilestonePlanVersion>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<MilestonePlanVersion?> GetLatestByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    Task<MilestonePlanVersion?> GetLatestByProposalIdAsync(Guid proposalId, CancellationToken ct = default);
    Task<MilestonePlanVersion?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<int> CountByProjectIdAsync(Guid projectId, CancellationToken ct = default);
}
