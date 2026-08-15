
namespace FreeGency.Application.Common.Interfaces;


public interface IEntitlementService
{
    // Active subscription's plan.
    Task<Plan?> GetCurrentPlanAsync(Guid userId, CancellationToken ct = default);

    // Soft gate: feature exists and is enabled on the user's plan.
    Task<EntitlementResult> CanAccessAsync(Guid userId, FeatureType feature, CancellationToken ct = default);

    // Hard gate: feature enabled AND quota remains.
    Task<EntitlementResult> CanConsumeAsync(Guid userId, FeatureType feature, CancellationToken ct = default);

    /// Hard gate + increments the UsageRecord.
    Task<EntitlementResult> ConsumeAsync(Guid userId, FeatureType feature, CancellationToken ct = default);


    Task<PlanSnapshotDto> GetSnapshotAsync(Guid userId, CancellationToken ct = default);
}