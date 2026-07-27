using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class RecentlyViewedPortfolioConfiguration : IEntityTypeConfiguration<RecentlyViewedPortfolio>
{
    public void Configure(EntityTypeBuilder<RecentlyViewedPortfolio> builder)
    {
        builder.ToTable("RecentlyViewedPortfolios", DbSchemas.Portfolio);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.PortfolioProjectId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ViewedAt });

        builder.Property(x => x.ViewedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(u => u.RecentlyViewedPortfolios)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PortfolioProject)
            .WithMany(p => p.RecentlyViewedByUsers)
            .HasForeignKey(x => x.PortfolioProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
