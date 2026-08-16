
namespace FreeGency.Application.Features.Plans;

public sealed record EntitlementResult(FeatureType Feature, bool IsAllowed, bool IsEnabled, int? Limit,
    int Used, int Remaining, string PlanName, string? Message = null)
{
    public AppError ToAppError()
    {
        if (IsAllowed)
            throw new AppInvalidOperationException("Entitlement is allowed; there is no error to return.");

        if (!IsEnabled)
            return AppError.PlanFeatureNotIncluded(Feature.ToString());

        if (AiFeaturePricing.IsTokenBased(Feature))
            return AppError.PlanTokenLimitReached(Feature.ToString(), Remaining);

        return AppError.PlanLimitReached(Feature.ToString(), Limit);
    }
}
