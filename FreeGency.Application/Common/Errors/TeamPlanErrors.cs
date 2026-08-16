using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class TeamPlanErrors
    {
        public static readonly Error NotTeamLeader =
            new(
                "TeamPlan.NotTeamLeader",
                "Only the team leader can change the team plan.",
                StatusCodes.Status403Forbidden);
        public static readonly Error InvalidPlanPrice =
    new(
        "TeamPlan.InvalidPlanPrice",
        "The selected plan does not have a valid price.",
        StatusCodes.Status400BadRequest);
        public static readonly Error SubscriptionNotFound =
            new(
                "TeamPlan.SubscriptionNotFound",
                "Team subscription was not found.",
                StatusCodes.Status404NotFound);

        public static readonly Error PlanNotFound =
            new(
                "TeamPlan.PlanNotFound",
                "Team plan was not found.",
                StatusCodes.Status404NotFound);

        public static readonly Error InsufficientBalance =
            new(
                "TeamPlan.InsufficientBalance",
                "Insufficient wallet balance.",
                StatusCodes.Status400BadRequest);
    }
}
