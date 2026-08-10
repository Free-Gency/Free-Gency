using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class DeveloperFeedbackConfiguration : IEntityTypeConfiguration<DeveloperFeedback>
{
    public void Configure(EntityTypeBuilder<DeveloperFeedback> builder)
    {
        builder.ToTable("DeveloperFeedbacks", DbSchemas.Identity);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.DeveloperUserId, x.ReviewerUserId }).IsUnique();
        builder.HasIndex(x => new { x.DeveloperUserId, x.CreatedAt });

        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(500);
        builder.Property(x => x.ModerationStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(ModerationStatus.Visible);
        builder.Property(x => x.ModerationNote).HasMaxLength(500);
        builder.Property(x => x.ModeratedText).HasMaxLength(500);

        builder.HasOne(x => x.DeveloperUser)
            .WithMany()
            .HasForeignKey(x => x.DeveloperUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict to avoid SQL Server multiple-cascade-path errors (both FKs → Users).
        builder.HasOne(x => x.ReviewerUser)
            .WithMany(u => u.DeveloperFeedbacks)
            .HasForeignKey(x => x.ReviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
