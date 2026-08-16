using FreeGency.Domain.Entities.TeamPlans;


namespace FreeGency.Infrastructure.Persistence.Configurations.plansTeam;

public class TeamUsageRecordConfiguration
 : IEntityTypeConfiguration<TeamUsageRecord>
{
    public void Configure(EntityTypeBuilder<TeamUsageRecord> builder)
    {
        builder.ToTable("TeamUsageRecords", "TeamPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Feature)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Used)
            .IsRequired();

        builder.Property(x => x.PeriodStart)
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.TeamSubscriptionId,
            x.Feature
        })
        .IsUnique();
    }
}
