using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class PortfolioProjectConfiguration : IEntityTypeConfiguration<PortfolioProject>
{
    public void Configure(EntityTypeBuilder<PortfolioProject> builder)
    {
        builder.ToTable("PortfolioProjects", DbSchemas.Portfolio);
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OwnerType).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Budget).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ImageCover).HasMaxLength(500);
        builder.Property(p => p.ProjectUrl).HasMaxLength(500);
        builder.Property(p => p.CompletionDate).HasColumnType("datetime2");
        builder.Property(p => p.Visibility)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Visibility.Public);

        builder.HasOne(p => p.OwnerUser)
            .WithMany(u => u.PortfolioProjects)
            .HasForeignKey(p => p.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(p => p.OwnerTeam)
            .WithMany(t => t.PortfolioProjects)
            .HasForeignKey(p => p.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.PortfolioProjects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
