using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Infrastructure.Persistence.Seeding;


namespace FreeGency.Infrastructure.Persistence.Configurations.plansTeam;

public class TeamPlanConfiguration
: IEntityTypeConfiguration<TeamPlan>
{
    public void Configure(EntityTypeBuilder<TeamPlan> builder)
    {
        builder.ToTable("TeamPlans", "TeamPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(350);

        builder.Property(x => x.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.YearlyPrice)
            .HasPrecision(18, 2);

        builder.HasMany(x => x.Features)
            .WithOne(x => x.TeamPlan)
            .HasForeignKey(x => x.TeamPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasData(TeamPlanSeeds.Plans);
    }
}
