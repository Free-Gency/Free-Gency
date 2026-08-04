using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamCategoryConfiguration : IEntityTypeConfiguration<TeamCategory>
{
    public void Configure(EntityTypeBuilder<TeamCategory> builder)
    {
        builder.ToTable("TeamCategories", DbSchemas.Teams);
        builder.HasKey(tc => new { tc.Id, tc.TeamId, tc.CategoryId });
        // A team may belong to multiple categories; one row per team+category.
        builder.HasIndex(tc => new { tc.TeamId, tc.CategoryId }).IsUnique();
        builder.HasIndex(tc => tc.TeamId);

        builder.Property(tc => tc.IsPrimary).HasDefaultValue(false);

        builder.HasOne(tc => tc.Team)
            .WithMany(t => t.TeamCategories)
            .HasForeignKey(tc => tc.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tc => tc.Category)
            .WithMany(c => c.TeamCategories)
            .HasForeignKey(tc => tc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
