
using FreeGency.Application.Features.PlanTeamFeature;
using FreeGency.Domain.Entities.TeamPlans;

namespace FreeGency.Application.Common.Interfaces;

public interface ITeamEntitlementService
{

    Task<TeamEntitlementResult> CanAccessTeamFeatureAsync(Guid teamId, TeamFeatureType feature, CancellationToken ct = default);

    Task<TeamEntitlementResult> CanConsumeTeamFeatureAsync(Guid teamId, TeamFeatureType feature, CancellationToken ct = default);

    Task<TeamEntitlementResult> ConsumeTeamFeatureAsync(Guid teamId, TeamFeatureType feature, CancellationToken ct = default);
}
