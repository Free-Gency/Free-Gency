
using FreeGency.Infrastructure.Persistence.Seeding;

namespace FreeGency.Infrastructure.Persistence.Configurations.plans;

public class PlanConfigrations : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.YearlyPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(p => p.Features)
            .WithOne(f => f.Plan)
            .HasForeignKey(f => f.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasData(

            new Plan
            {
                Id = PlanSeeds.FreePlanId,

                Name = "Free",
                Description = "Essential features to get started.",

                MonthlyPrice = 0,
                YearlyPrice = 0,

                IsActive = true,

                CreatedAt = PlanSeeds.SeedDate,
                CreatedBy = "System",

                IsDeleted = false
            },
            new Plan
            {
                Id = PlanSeeds.PremiumPlanId,

                Name = "Premium",
                Description = "More projects, proposals and AI features.",

                MonthlyPrice = 10,
                YearlyPrice = 100,

                IsActive = true,

                CreatedAt = PlanSeeds.SeedDate,
                CreatedBy = "System",

                IsDeleted = false
            },
             new Plan
             {
                 Id = PlanSeeds.ProPlanId,

                 Name = "Pro",
                 Description = "Maximum limits and full access to advanced features.",

                 MonthlyPrice = 25,
                 YearlyPrice = 250,

                 IsActive = true,

                 CreatedAt = PlanSeeds.SeedDate,
                 CreatedBy = "System",

                 IsDeleted = false
             }
        );
    }
}
