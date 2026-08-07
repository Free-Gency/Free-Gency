using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamFeedbackConfiguration : IEntityTypeConfiguration<TeamFeedback>
{
    public void Configure(EntityTypeBuilder<TeamFeedback> builder)
    {
        builder.ToTable("TeamFeedbacks", DbSchemas.Teams);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TeamId, x.ReviewerUserId }).IsUnique();
        builder.HasIndex(x => new { x.TeamId, x.CreatedAt });

        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(500);

        builder.HasOne(x => x.Team)
            .WithMany(t => t.TeamFeedbacks)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewerUser)
            .WithMany(u => u.TeamFeedbacks)
            .HasForeignKey(x => x.ReviewerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
