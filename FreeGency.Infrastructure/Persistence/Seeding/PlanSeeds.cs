using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Seeding
{
    public static class PlanSeeds
    {
        public static readonly Guid FreePlanId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        public static readonly Guid PremiumPlanId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        public static readonly Guid ProPlanId =
            Guid.Parse("33333333-3333-3333-3333-333333333333");

        public static readonly DateTime SeedDate =
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
