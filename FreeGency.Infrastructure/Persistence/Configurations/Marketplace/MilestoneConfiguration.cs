using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestones", DbSchemas.Marketplace);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired();
        builder.Property(m => m.Amount).HasColumnType("decimal(18,2)");
        builder.Property(m => m.ReleasedAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(m => m.SortOrder).HasDefaultValue(0);
        builder.Property(m => m.DueDate).HasColumnType("datetime2");
        builder.Property(m => m.IsFunded).HasDefaultValue(false);
        builder.Property(m => m.ReleaseStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ReleaseStatus.Locked);
        builder.Property(m => m.WorkStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(WorkStatus.NotStarted);
        builder.Property(m => m.ProposedByUserId).HasMaxLength(256);
        builder.Property(m => m.SubmittedAt).HasColumnType("datetime2");
        builder.Property(m => m.AvailableAt).HasColumnType("datetime2");
        builder.Property(m => m.ReleasedAt).HasColumnType("datetime2");

        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
