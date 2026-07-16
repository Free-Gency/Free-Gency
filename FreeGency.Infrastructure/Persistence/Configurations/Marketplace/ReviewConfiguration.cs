using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", DbSchemas.Marketplace);
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RevieweeType).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Comment).HasMaxLength(500);

        builder.HasIndex(r => new { r.ProjectId, r.ReviewerUserId, r.RevieweeType }).IsUnique();

        builder.HasOne(r => r.Project)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReviewerUser)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RevieweeTeam)
            .WithMany(t => t.Reviews)
            .HasForeignKey(r => r.RevieweeTeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(r => r.RevieweeUser)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.RevieweeUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
