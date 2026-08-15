using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IHirePyEvaluationRepository : IGenericRepository<HirePyEvaluation>
{
    Task<IReadOnlyList<HirePyEvaluation>> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken ct = default);

    Task<IReadOnlyList<HirePyEvaluation>> GetByProposalIdAsync(
        Guid proposalId,
        CancellationToken ct = default);
}
