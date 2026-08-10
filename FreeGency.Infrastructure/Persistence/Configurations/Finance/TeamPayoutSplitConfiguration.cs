using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Finance;

public class TeamPayoutSplitConfiguration : IEntityTypeConfiguration<TeamPayoutSplit>
{
    public void Configure(EntityTypeBuilder<TeamPayoutSplit> builder)
    {
        builder.ToTable("TeamPayoutSplits", DbSchemas.Finance);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SplitType).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Value).HasColumnType("decimal(18,2)");

        builder.HasOne(s => s.Team)
            .WithMany(t => t.TeamPayoutSplits)
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Project)
            .WithMany(p => p.TeamPayoutSplits)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(s => s.Milestone)
            .WithMany(m => m.TeamPayoutSplits)
            .HasForeignKey(s => s.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(s => s.User)
            .WithMany(u => u.TeamPayoutSplits)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.TeamId, s.ProjectId, s.MilestoneId, s.UserId });
    }
}
