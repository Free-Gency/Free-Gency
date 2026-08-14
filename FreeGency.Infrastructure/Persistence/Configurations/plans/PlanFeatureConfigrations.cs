
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
            .HasDefaultValue(true);

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
    }
}
