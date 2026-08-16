using FreeGency.Application.Common.Models;

namespace FreeGency.Application.Common.Errors;

public static class PlanErrors
{
    public static readonly Error NotFound =
        new("Plan.NotFound", "Plan was not found.", StatusCodes.Status404NotFound);
    public static readonly Error AlreadySubscribed =
        new("Plan.AlreadySubscribed", "You already have an active subscription to this plan.", StatusCodes.Status409Conflict);
}