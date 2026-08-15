using FreeGency.Application.Features.HirePy.Dtos;

namespace FreeGency.Application.Common.Interfaces;

/// <summary>
/// Presents the final client-safe recommendation and executes the client's "Approve &amp; Hire"
/// decision. The hire itself is delegated to the existing hiring service; this service only verifies
/// ownership, eligibility and availability before calling it.
/// </summary>
public interface IHirePyApprovalService
{
    /// <summary>Returns the client-safe recommendation summary for a session (client-only).</summary>
    Task<ApiResponse<HirePyRecommendationDto>> GetRecommendationAsync(
        Guid sessionId,
        CancellationToken ct = default);

    /// <summary>
    /// Approves the recommendation and hires the recommended candidate through the existing hiring
    /// service. Verifies client authentication, ownership, session, recommendation, selected
    /// candidate, candidate eligibility and project availability before the hire.
    /// </summary>
    Task<ApiResponse> ApproveAndHireAsync(
        Guid sessionId,
        CancellationToken ct = default);
}
