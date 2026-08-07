using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers", DbSchemas.Teams);
        builder.HasKey(tm => new { tm.Id, tm.TeamId, tm.UserId });
        builder.HasIndex(tm => new { tm.TeamId, tm.UserId }).IsUnique();
        builder.HasIndex(tm => tm.UserId);

        builder.Property(tm => tm.TeamRole).HasConversion<string>().HasMaxLength(50);
        builder.Property(tm => tm.Job).HasMaxLength(100);
        builder.Property(tm => tm.JoinedAt).HasMaxLength(50);

        builder.HasOne(tm => tm.Team)
            .WithMany(t => t.TeamMembers)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tm => tm.User)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
