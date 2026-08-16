using FreeGency.Domain.Entities.TeamPlans;

namespace FreeGency.Infrastructure.Persistence.Seeding;

public static class TeamPlanSeeds
{
    public static readonly Guid FreeTeamPlanId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid ProTeamPlanId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid PremiumTeamPlanId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid FreeCreateProposalFeatureId =
        Guid.Parse("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid ProCreateProposalFeatureId =
        Guid.Parse("22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid PremiumCreateProposalFeatureId =
        Guid.Parse("33333333-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly TeamPlan[] Plans =
    [
        new TeamPlan
        {
            Id = FreeTeamPlanId,
            Name = "Free",
            MonthlyPrice = 0,
            YearlyPrice = 0,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlan
        {
            Id = ProTeamPlanId,
            Name = "Pro",
            MonthlyPrice = 20,
            YearlyPrice = 200,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlan
        {
            Id = PremiumTeamPlanId,
            Name = "Premium",
            MonthlyPrice = 50,
            YearlyPrice = 500,
            CreatedAt = new DateTime(2026, 1, 1)
        }
    ];

    public static readonly TeamPlanFeature[] Features =
    [
        new TeamPlanFeature
        {
            Id = FreeCreateProposalFeatureId,
            TeamPlanId = FreeTeamPlanId,
            Feature = TeamFeatureType.CreateProposal,
            Limit = 5,
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlanFeature
        {
            Id = ProCreateProposalFeatureId,
            TeamPlanId = ProTeamPlanId,
            Feature = TeamFeatureType.CreateProposal,
            Limit = 30,
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlanFeature
        {
            Id = PremiumCreateProposalFeatureId,
            TeamPlanId = PremiumTeamPlanId,
            Feature = TeamFeatureType.CreateProposal,
            Limit = 70, // Unlimited
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        }
    ];
}