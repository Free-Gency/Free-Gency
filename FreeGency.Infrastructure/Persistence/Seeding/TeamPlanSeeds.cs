using FreeGency.Domain.Entities.TeamPlans;

namespace FreeGency.Infrastructure.Persistence.Seeding;

public static class TeamPlanSeeds
{
    public static readonly Guid FreeTeamPlanId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid PremiumTeamPlanId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid ProTeamPlanId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");


        // Free Features ID
    public static readonly Guid FreeCreateProposalFeatureId =
        Guid.Parse("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid FreeTeamMembersFeatureId =
        Guid.Parse("11111111-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid FreeAIGenerateMileStoneFeatureId =
        Guid.Parse("11111111-cccc-cccc-cccc-cccccccccccc");


        // Premium Features ID
    public static readonly Guid PremiumCreateProposalFeatureId =
        Guid.Parse("22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid PremiumTeamMembersFeatureId =
        Guid.Parse("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid PremiumAIGenerateMileStoneFeatureId =
        Guid.Parse("22222222-cccc-cccc-cccc-cccccccccccc");


    // Pro Features ID
    public static readonly Guid ProCreateProposalFeatureId =
        Guid.Parse("33333333-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid ProTeamMembersFeatureId =
        Guid.Parse("33333333-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid ProAIGenerateMileStoneFeatureId =
        Guid.Parse("33333333-cccc-cccc-cccc-cccccccccccc");

    public static readonly TeamPlan[] Plans =
    [
        new TeamPlan
        {
            Id = FreeTeamPlanId,
            Name = "Free",
            Description = "Perfect for small teams just getting started",
            MonthlyPrice = 0,
            YearlyPrice = 0,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlan
        {
            Id = PremiumTeamPlanId,
            Name = "Premium",
            Description = "Great for growing teams",
            MonthlyPrice = 5,
            YearlyPrice = 50,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlan
        {
            Id = ProTeamPlanId,
            Name = "Pro",
            Description = "Perfect for large teams",
            MonthlyPrice = 10,
            YearlyPrice = 100,
            CreatedAt = new DateTime(2026, 1, 1)
        }
    ];

    public static readonly TeamPlanFeature[] Features =
    [
        // Free
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
            Id = FreeTeamMembersFeatureId,
            TeamPlanId = FreeTeamPlanId,
            Feature = TeamFeatureType.TeamMembers,
            Limit = 5,
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        //new TeamPlanFeature
        //{
        //    Id = FreeAIGenerateMileStoneFeatureId,
        //    TeamPlanId = FreeTeamPlanId,
        //    Feature = TeamFeatureType.AIGenerateMilestone,
        //    Limit = 1,
        //    IsEnabled = true,
        //    CreatedAt = new DateTime(2026, 1, 1)
        //},


        // Premium
        new TeamPlanFeature
        {
            Id = PremiumCreateProposalFeatureId,
            TeamPlanId = PremiumTeamPlanId,
            Feature = TeamFeatureType.CreateProposal,
            Limit = 15,
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlanFeature
        {
            Id = PremiumTeamMembersFeatureId,
            TeamPlanId = PremiumTeamPlanId,
            Feature = TeamFeatureType.TeamMembers,
            Limit = 15,
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        //new TeamPlanFeature
        //{
        //    Id = PremiumAIGenerateMileStoneFeatureId,
        //    TeamPlanId = PremiumTeamPlanId,
        //    Feature = TeamFeatureType.AIGenerateMilestone,
        //    Limit = 10,
        //    IsEnabled = true,
        //    CreatedAt = new DateTime(2026, 1, 1)
        //},


        // Pro
        new TeamPlanFeature
        {
            Id = ProCreateProposalFeatureId,
            TeamPlanId = ProTeamPlanId,
            Feature = TeamFeatureType.CreateProposal,
            Limit = 70, // Unlimited
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        },

        new TeamPlanFeature
        {
            Id = ProTeamMembersFeatureId,
            TeamPlanId = ProTeamPlanId,
            Feature = TeamFeatureType.TeamMembers,
            Limit = 70,
            IsEnabled = true,
            CreatedAt = new DateTime(2026, 1, 1)
        }

        //new TeamPlanFeature
        //{
        //    Id = ProAIGenerateMileStoneFeatureId,
        //    TeamPlanId = ProTeamPlanId,
        //    Feature = TeamFeatureType.AIGenerateMilestone,
        //    Limit = 2500,
        //    IsEnabled = true,
        //    CreatedAt = new DateTime(2026, 1, 1)
        //}
    ];
}