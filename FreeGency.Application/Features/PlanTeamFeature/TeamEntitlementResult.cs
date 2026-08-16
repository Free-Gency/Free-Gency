
using FreeGency.Domain.Entities.TeamPlans;

namespace FreeGency.Application.Features.PlanTeamFeature;

public sealed record TeamEntitlementResult(
    TeamFeatureType Feature,
    bool IsAllowed,
    bool IsEnabled,
    int? Limit,
    int Used,
    int Remaining,
    string PlanName,
    string? Message = null)
{
    public AppError ToAppError()
    {
        if (IsAllowed)
            throw new AppInvalidOperationException("Team entitlement is allowed; there is no error to return.");

        return new AppError(
            IsEnabled ? "TeamPlan.LimitReached" : "TeamPlan.NotIncluded",
            Message ?? (IsEnabled
                ? $"You have reached your {Feature} limit for this period."
                : $"{Feature} is not included in your current team plan."),
            StatusCodes.Status403Forbidden);
    }
}
