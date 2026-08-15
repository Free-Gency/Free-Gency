
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

            // =================================================
            // FREE
            // =================================================

            Feature(
                "11111111-0000-0000-0000-000000000001",
                PlanSeeds.FreePlanId,
                FeatureType.CreateProject,
                2,
                true),

            Feature(
                "11111111-0000-0000-0000-000000000002",
                PlanSeeds.FreePlanId,
                FeatureType.SendProposal,
                5
                , true),

            Feature(
                "11111111-0000-0000-0000-000000000003",
                PlanSeeds.FreePlanId,
                FeatureType.TeamMembers,
                3, true),
            Feature(
                "11111111-0000-0000-0000-000000000004",
                PlanSeeds.FreePlanId,
                FeatureType.ActiveProjects,
                1, true),

            Feature(
                "11111111-0000-0000-0000-000000000005",
                PlanSeeds.FreePlanId,
                FeatureType.FileStorage,
                100, true),

            Feature(
                "11111111-0000-0000-0000-000000000006",
                PlanSeeds.FreePlanId,
                FeatureType.SendInvitation,
                5, true),

            Feature(
                "11111111-0000-0000-0000-000000000007",
                PlanSeeds.FreePlanId,
                FeatureType.GenerateProjectDraft,
                3, true),
             Feature(
                "11111111-0000-0000-0000-000000000008",
                PlanSeeds.FreePlanId,
                FeatureType.TeamSuggestions,
                3, true),

            Feature(
                "11111111-0000-0000-0000-000000000009",
                PlanSeeds.FreePlanId,
                FeatureType.AIChat,
                10, true),

            Feature(
                "11111111-0000-0000-0000-000000000010",
                PlanSeeds.FreePlanId,
                FeatureType.HiringAgent,
                null,
                false),
            //premium
            Feature(
                "22222222-0000-0000-0000-000000000001",
                PlanSeeds.PremiumPlanId,
                FeatureType.CreateProject,
                10, true),

            Feature(
                "22222222-0000-0000-0000-000000000002",
                PlanSeeds.PremiumPlanId,
                FeatureType.SendProposal,
                30, true),

            Feature(
                "22222222-0000-0000-0000-000000000003",
                PlanSeeds.PremiumPlanId,
                FeatureType.TeamMembers,
                10, true),

            Feature(
                "22222222-0000-0000-0000-000000000004",
                PlanSeeds.PremiumPlanId,
                FeatureType.ActiveProjects,
                5, true),
            Feature(
                "22222222-0000-0000-0000-000000000005",
                PlanSeeds.PremiumPlanId,
                FeatureType.FileStorage,
                2048, true),

            Feature(
                "22222222-0000-0000-0000-000000000006",
                PlanSeeds.PremiumPlanId,
                FeatureType.SendInvitation,
                30, true),

            Feature(
                "22222222-0000-0000-0000-000000000007",
                PlanSeeds.PremiumPlanId,
                FeatureType.GenerateProjectDraft,
                20, true),

            Feature(
                "22222222-0000-0000-0000-000000000008",
                PlanSeeds.PremiumPlanId,
                FeatureType.TeamSuggestions,
                20, true),

            Feature(
                "22222222-0000-0000-0000-000000000009",
                PlanSeeds.PremiumPlanId,
                FeatureType.AIChat,
                100, true),
            Feature(
                "22222222-0000-0000-0000-000000000010",
                PlanSeeds.PremiumPlanId,
                FeatureType.HiringAgent,
                10, true),
            Feature(
                "33333333-0000-0000-0000-000000000001",
                PlanSeeds.ProPlanId,
                FeatureType.CreateProject,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000002",
                PlanSeeds.ProPlanId,
                FeatureType.SendProposal,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000003",
                PlanSeeds.ProPlanId,
                FeatureType.TeamMembers,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000004",
                PlanSeeds.ProPlanId,
                FeatureType.ActiveProjects,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000005",
                PlanSeeds.ProPlanId,
                FeatureType.FileStorage,
                10240, true),
            Feature(
                "33333333-0000-0000-0000-000000000006",
                PlanSeeds.ProPlanId,
                FeatureType.SendInvitation,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000007",
                PlanSeeds.ProPlanId,
                FeatureType.GenerateProjectDraft,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000008",
                PlanSeeds.ProPlanId,
                FeatureType.TeamSuggestions,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000009",
                PlanSeeds.ProPlanId,
                FeatureType.AIChat,
                null, true),

            Feature(
                "33333333-0000-0000-0000-000000000010",
                PlanSeeds.ProPlanId,
                FeatureType.HiringAgent,
                null, true)
            );
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

