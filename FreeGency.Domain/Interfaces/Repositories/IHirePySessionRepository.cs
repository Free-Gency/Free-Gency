using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IHirePySessionRepository : IGenericRepository<HirePySession>
{
    Task<IReadOnlyList<HirePySession>> GetByClientUserIdAsync(
        Guid clientUserId,
        CancellationToken ct = default);

    Task<HirePySession?> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken ct = default);

    /// <summary>Sessions whose candidate discussions are underway and may be ready for evaluation.</summary>
    Task<IReadOnlyList<HirePySession>> GetPendingEvaluationAsync(CancellationToken ct = default);

    /// <summary>
    /// Atomically transitions a session from one status to another (concurrency-safe conditional
    /// update). Returns <see langword="false"/> when the session is not in the expected source
    /// status, so concurrent approvals cannot both proceed.
    /// </summary>
    Task<bool> TryTransitionStatusAsync(
        Guid sessionId,
        HirePySessionStatus from,
        HirePySessionStatus to,
        CancellationToken ct = default);
}
