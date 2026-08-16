using FreeGency.Domain.Entities.TeamPlans;


namespace FreeGency.Infrastructure.Persistence.Configurations.plansTeam;

public class TeamSubscriptionConfiguration
 : IEntityTypeConfiguration<TeamSubscription>
{
    public void Configure(EntityTypeBuilder<TeamSubscription> builder)
    {
        builder.ToTable("TeamSubscriptions", "TeamPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BillingPeriod)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.AutoRenew)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasOne(x => x.Team)
            .WithOne(x => x.Subscription)
            .HasForeignKey<TeamSubscription>(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TeamPlan)
            .WithMany()
            .HasForeignKey(x => x.TeamPlanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.UsageRecords)
       .WithOne(x => x.Subscription)
       .HasForeignKey(x => x.TeamSubscriptionId)
       .OnDelete(DeleteBehavior.Cascade);
    }
}
