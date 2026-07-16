using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamJobConfiguration : IEntityTypeConfiguration<TeamJob>
{
    public void Configure(EntityTypeBuilder<TeamJob> builder)
    {
        builder.ToTable("TeamJobs", DbSchemas.Teams);
        builder.HasKey(tj => tj.Id);

        builder.Property(tj => tj.Title).IsRequired().HasMaxLength(200);
        builder.Property(tj => tj.Description).IsRequired();
        builder.Property(tj => tj.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(TeamJobStatus.open);
        builder.Property(tj => tj.ClosedAt).HasColumnType("datetime2");

        builder.HasOne(tj => tj.Team)
            .WithMany(t => t.TeamJobs)
            .HasForeignKey(tj => tj.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tj => tj.CreatedByUser)
            .WithMany(u => u.CreatedTeamJobs)
            .HasForeignKey(tj => tj.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
