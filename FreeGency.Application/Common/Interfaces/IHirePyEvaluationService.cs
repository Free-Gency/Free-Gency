using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.HirePy.Dtos;

namespace FreeGency.Application.Common.Interfaces;

/// <summary>
/// Evaluates every candidate who finalized their milestone plan for a HirePy session and selects the
/// single recommended candidate. Drives the CandidateEvaluation → CandidateComparison →
/// RecommendationReady → WaitingForClientApproval transition and notifies the client.
/// </summary>
public interface IHirePyEvaluationService
{
    /// <summary>Polls sessions whose discussions are concluded and processes the evaluation when all interviews are done.</summary>
    Task ProcessPendingEvaluationsAsync(CancellationToken ct = default);

    /// <summary>Returns the structured evaluation rows for a session (client-safe, owner only).</summary>
    Task<ApiResponse<IReadOnlyList<HirePyEvaluationDto>>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
}
