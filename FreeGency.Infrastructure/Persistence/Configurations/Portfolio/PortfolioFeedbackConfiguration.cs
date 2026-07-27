using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class PortfolioFeedbackConfiguration : IEntityTypeConfiguration<PortfolioFeedback>
{
    public void Configure(EntityTypeBuilder<PortfolioFeedback> builder)
    {
        builder.ToTable("PortfolioFeedbacks", DbSchemas.Portfolio);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.PortfolioProjectId, x.ReviewerUserId }).IsUnique();
        builder.HasIndex(x => new { x.PortfolioProjectId, x.CreatedAt });

        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(500);

        builder.HasOne(x => x.PortfolioProject)
            .WithMany(p => p.Feedbacks)
            .HasForeignKey(x => x.PortfolioProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewerUser)
            .WithMany(u => u.PortfolioFeedbacks)
            .HasForeignKey(x => x.ReviewerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
