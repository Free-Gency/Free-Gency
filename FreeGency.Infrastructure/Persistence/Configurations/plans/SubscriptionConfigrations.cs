
namespace FreeGency.Infrastructure.Persistence.Configurations.plans;

public class SubscriptionConfigrations : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.AutoRenew)
            .HasDefaultValue(false);

        // User 1 : 1 Subscription
        builder.HasOne(x => x.User)
            .WithOne(x => x.Subscriptions)
            .HasForeignKey<Subscription>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plan 1 : Many Subscriptions
        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
