using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IHirePyInterviewRepository : IGenericRepository<HirePyInterview>
{
    /// <summary>Active interviews that are not currently being processed by another worker.</summary>
    Task<List<HirePyInterview>> GetPendingAsync(CancellationToken ct = default);

    Task<HirePyInterview?> GetByProposalIdAsync(Guid proposalId, CancellationToken ct = default);

    Task<IReadOnlyList<HirePyInterview>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
}
