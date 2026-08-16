namespace FreeGency.Application.Features.Plans.Dtos;

public sealed record ChangePlanRequestDto(Guid PlanId, BillingPeriod BillingPeriod);