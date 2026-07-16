using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Finance;

public class EscrowHoldConfiguration : IEntityTypeConfiguration<EscrowHold>
{
    public void Configure(EntityTypeBuilder<EscrowHold> builder)
    {
        builder.ToTable("EscrowHolds", DbSchemas.Finance);
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.ProjectId).IsUnique();

        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(e => e.TotalReleased).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(e => e.FundingStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(FundingStatus.Unlocked);
        builder.Property(e => e.planStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PlanStatus.AwaitingPlan);
        builder.Property(e => e.LockedAt).HasColumnType("datetime2");
        builder.Property(e => e.PlanAgreedAt).HasColumnType("datetime2");

        builder.HasOne(e => e.Project)
            .WithOne(p => p.EscrowHold)
            .HasForeignKey<EscrowHold>(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
