using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams", DbSchemas.Teams);
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Logo).HasMaxLength(500);
        builder.Property(t => t.Cover).HasMaxLength(500);
        builder.Property(t => t.TeamCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TeamCode).IsUnique();
        builder.HasIndex(t => t.OwnerUserId);
        builder.Property(t => t.AboutUs).HasMaxLength(2000);
        builder.Property(t => t.AverageRating).HasColumnType("decimal(3,2)").HasDefaultValue(0m);
        builder.Property(t => t.RatingCount).HasDefaultValue(0);
    }
}
