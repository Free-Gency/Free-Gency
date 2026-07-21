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
        // Enforce: each team belongs to exactly one category (Category -> Team is 1 to M)
        builder.HasIndex(tc => tc.TeamId).IsUnique();
        builder.HasIndex(tc => new { tc.TeamId, tc.CategoryId }).IsUnique();

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
