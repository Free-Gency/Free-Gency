
using FreeGency.Infrastructure.Persistence.Seeding;

namespace FreeGency.Infrastructure.Persistence.Configurations.plans;

public class PlanFeatureConfigrations : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Feature)
            .IsRequired();

        builder.Property(x => x.Limit)
            .IsRequired(false);

        builder.Property(x => x.IsEnabled)
     .IsRequired();

        builder.HasIndex(x => new
        {
            x.PlanId,
            x.Feature
        })
        .IsUnique();

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Features)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasData(

        #region Free Plan
            Feature(
                "11111111-0000-0000-0000-000000000001",
                PlanSeeds.FreePlanId,
                FeatureType.CreateProject,
                3,
                true),

            Feature(
                "11111111-0000-0000-0000-000000000002",
                PlanSeeds.FreePlanId,
                FeatureType.SendProposal,
                3
                , true),

            Feature(
                "11111111-0000-0000-0000-000000000003",
                PlanSeeds.FreePlanId,
                FeatureType.JoinedTeams,
                3, true),
            Feature(
                "11111111-0000-0000-0000-000000000004",
                PlanSeeds.FreePlanId,
                FeatureType.ActiveProjects,
                2, true),

            Feature(
                "11111111-0000-0000-0000-000000000005",
                PlanSeeds.FreePlanId,
                FeatureType.ProjectSendInvitation,
                5, true),

            Feature(
                "11111111-0000-0000-0000-000000000006",
                PlanSeeds.FreePlanId,
                FeatureType.TeamInvite,
                10, true),

            Feature(
                "11111111-0000-0000-0000-000000000007",
                PlanSeeds.FreePlanId,
                FeatureType.GenerateProjectDraft,
                5000, true),
             Feature(
                "11111111-0000-0000-0000-000000000008",
                PlanSeeds.FreePlanId,
                FeatureType.TeamSuggestions,
                2000, true),

            Feature(
                "11111111-0000-0000-0000-000000000009",
                PlanSeeds.FreePlanId,
                FeatureType.AIChatProposal,
                2500, true),

                Feature(
                "11111111-0000-0000-0000-000000000010",
                PlanSeeds.FreePlanId,
                FeatureType.ProposalRanking,
                3500, true),

            Feature(
                "11111111-0000-0000-0000-000000000011",
                PlanSeeds.FreePlanId,
                FeatureType.HiringAgent,
                null,
                false),
        #endregion

        #region Premium Plan
            Feature(
                "22222222-0000-0000-0000-000000000001",
                PlanSeeds.PremiumPlanId,
                FeatureType.CreateProject,
                20, true),

            Feature(
                "22222222-0000-0000-0000-000000000002",
                PlanSeeds.PremiumPlanId,
                FeatureType.SendProposal,
                10, true), // per day

            Feature(
                "22222222-0000-0000-0000-000000000003",
                PlanSeeds.PremiumPlanId,
                FeatureType.JoinedTeams,
                10, true),

            Feature(
                "22222222-0000-0000-0000-000000000004",
                PlanSeeds.PremiumPlanId,
                FeatureType.ActiveProjects,
                10, true),
            Feature(
                "22222222-0000-0000-0000-000000000005",
                PlanSeeds.PremiumPlanId,
                FeatureType.ProjectSendInvitation,
                30, true),

            Feature(
                "22222222-0000-0000-0000-000000000006",
                PlanSeeds.PremiumPlanId,
                FeatureType.TeamInvite,
                50, true),

            Feature(
                "22222222-0000-0000-0000-000000000007",
                PlanSeeds.PremiumPlanId,
                FeatureType.GenerateProjectDraft,
                5000, true),

            Feature(
                "22222222-0000-0000-0000-000000000008",
                PlanSeeds.PremiumPlanId,
                FeatureType.TeamSuggestions,
                2000, true),

            Feature(
                "22222222-0000-0000-0000-000000000009",
                PlanSeeds.PremiumPlanId,
                FeatureType.AIChatProposal,
                2500, true),

            Feature(
                "22222222-0000-0000-0000-000000000010",
                PlanSeeds.PremiumPlanId,
                FeatureType.ProposalRanking,
                3500, true),
            Feature(
                "22222222-0000-0000-0000-000000000011",
                PlanSeeds.PremiumPlanId,
                FeatureType.HiringAgent,
                10_000, true),
        #endregion

        #region Pro Plan
            Feature(
                "33333333-0000-0000-0000-000000000001",
                PlanSeeds.ProPlanId,
                FeatureType.CreateProject,
                150, true),

            Feature(
                "33333333-0000-0000-0000-000000000002",
                PlanSeeds.ProPlanId,
                FeatureType.SendProposal,
                30, true), // per day

            Feature(
                "33333333-0000-0000-0000-000000000003",
                PlanSeeds.ProPlanId,
                FeatureType.JoinedTeams,
                25, true),

            Feature(
                "33333333-0000-0000-0000-000000000004",
                PlanSeeds.ProPlanId,
                FeatureType.ActiveProjects,
                30, true),

            Feature(
                "33333333-0000-0000-0000-000000000005",
                PlanSeeds.ProPlanId,
                FeatureType.ProjectSendInvitation,
                100, true),
            Feature(
                "33333333-0000-0000-0000-000000000006",
                PlanSeeds.ProPlanId,
                FeatureType.TeamInvite,
                150, true),

            Feature(
                "33333333-0000-0000-0000-000000000007",
                PlanSeeds.ProPlanId,
                FeatureType.GenerateProjectDraft,
                5000, true),

            Feature(
                "33333333-0000-0000-0000-000000000008",
                PlanSeeds.ProPlanId,
                FeatureType.TeamSuggestions,
                2000, true),

            Feature(
                "33333333-0000-0000-0000-000000000009",
                PlanSeeds.ProPlanId,
                FeatureType.AIChatProposal,
                2500, true),

            Feature(
                "33333333-0000-0000-0000-000000000010",
                PlanSeeds.ProPlanId,
                FeatureType.ProposalRanking,
                3500, true),

            Feature(
                "33333333-0000-0000-0000-000000000011",
                PlanSeeds.ProPlanId,
                FeatureType.HiringAgent,
                10_000, true)
            );
        #endregion
    }
    private static PlanFeature Feature(
        string id,
        Guid planId,
        FeatureType feature,
        int? limit,
        bool isEnabled)
    {
        return new PlanFeature
        {
            Id = Guid.Parse(id),

            PlanId = planId,
            Feature = feature,
            Limit = limit,
            IsEnabled = isEnabled,

            CreatedAt = PlanSeeds.SeedDate,
            CreatedBy = "System",

            IsDeleted = false
        };
    }
}

