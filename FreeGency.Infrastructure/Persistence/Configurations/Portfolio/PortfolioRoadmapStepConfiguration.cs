using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class PortfolioRoadmapStepConfiguration : IEntityTypeConfiguration<PortfolioRoadmapStep>
{
    public void Configure(EntityTypeBuilder<PortfolioRoadmapStep> builder)
    {
        builder.ToTable("PortfolioRoadmapSteps", DbSchemas.Portfolio);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.IsDone).HasDefaultValue(false);

        builder.HasIndex(x => new { x.PortfolioProjectId, x.SortOrder });

        builder.HasOne(x => x.PortfolioProject)
            .WithMany(p => p.RoadmapSteps)
            .HasForeignKey(x => x.PortfolioProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
