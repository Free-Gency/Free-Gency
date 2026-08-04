using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class PortfolioMetricConfiguration : IEntityTypeConfiguration<PortfolioMetric>
{
    public void Configure(EntityTypeBuilder<PortfolioMetric> builder)
    {
        builder.ToTable("PortfolioMetrics", DbSchemas.Portfolio);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Label).IsRequired().HasMaxLength(120);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        builder.HasIndex(x => new { x.PortfolioProjectId, x.SortOrder });

        builder.HasOne(x => x.PortfolioProject)
            .WithMany(p => p.Metrics)
            .HasForeignKey(x => x.PortfolioProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
