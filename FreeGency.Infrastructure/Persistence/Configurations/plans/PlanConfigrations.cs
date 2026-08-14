
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
    }
}
