using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class PortfolioImageConfiguration : IEntityTypeConfiguration<PortfolioImage>
{
    public void Configure(EntityTypeBuilder<PortfolioImage> builder)
    {
        builder.ToTable("PortfolioImages", DbSchemas.Portfolio);
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(i => i.SortOrder).HasDefaultValue(0);

        builder.HasOne(i => i.PortfolioProject)
            .WithMany(p => p.PortfolioImages)
            .HasForeignKey(i => i.PortfolioProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
