
namespace FreeGency.Application.Common.Interfaces;


public interface IEntitlementService
{
    /// Active subscription's plan, else the seeded Free plan.
    Task<Plan?> GetCurrentPlanAsync(Guid userId, CancellationToken ct = default);

    /// Soft gate: feature exists and is enabled on the user's plan (UI enablement, ignores quota).
    Task<EntitlementResult> CanAccessAsync(Guid userId, FeatureType feature, CancellationToken ct = default);

    /// Hard gate: feature enabled AND quota remains. No mutation. teamId = counting identity for SendProposal when a team applies.
    Task<EntitlementResult> CanConsumeAsync(Guid userId, FeatureType feature, CancellationToken ct = default);

    /// Hard gate + increments the UsageRecord bucket (AI features only; table-counted features just re-check).
    Task<EntitlementResult> ConsumeAsync(Guid userId, FeatureType feature, CancellationToken ct = default);

    /// Full usage snapshot for GET /api/v1/billing/me.
    Task<PlanSnapshotDto> GetSnapshotAsync(Guid userId, CancellationToken ct = default);
}