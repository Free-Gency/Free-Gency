using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamJoinRequestConfiguration : IEntityTypeConfiguration<TeamJoinRequest>
{
    public void Configure(EntityTypeBuilder<TeamJoinRequest> builder)
    {
        builder.ToTable("TeamJoinRequests", DbSchemas.Teams);
        builder.HasKey(tjr => tjr.Id);

        builder.Property(tjr => tjr.Job).HasMaxLength(100);
        builder.Property(tjr => tjr.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(TeamJoinRequestStatus.pending);
        builder.Property(tjr => tjr.RequestedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(tjr => tjr.ResponseAt).HasColumnType("datetime2");
        builder.Property(tjr => tjr.RespondedByUserId).HasMaxLength(256);

        builder.HasOne(tjr => tjr.Team)
            .WithMany(t => t.TeamJoinRequests)
            .HasForeignKey(tjr => tjr.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tjr => tjr.TeamJob)
            .WithMany(tj => tj.TeamJoinRequests)
            .HasForeignKey(tjr => tjr.TeamJobId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(tjr => tjr.User)
            .WithMany(u => u.TeamJoinRequests)
            .HasForeignKey(tjr => tjr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
