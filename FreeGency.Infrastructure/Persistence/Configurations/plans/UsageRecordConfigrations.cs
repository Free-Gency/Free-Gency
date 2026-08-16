
namespace FreeGency.Infrastructure.Persistence.Configurations.plans;

public class UsageRecordConfigrations : IEntityTypeConfiguration<UsageRecord>
{
    public void Configure(EntityTypeBuilder<UsageRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Used)
            .HasDefaultValue(0);

        builder.Property(x => x.TokensUsed)
        .HasDefaultValue(0);

        builder.HasIndex(x => new
        {
            x.SubscriptionId,
            x.Feature
        })
        .IsUnique();

        builder.HasOne(x => x.subscription)
            .WithMany(x => x.UsageRecords)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
