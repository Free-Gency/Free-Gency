using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class DeveloperProfileConfiguration : IEntityTypeConfiguration<DeveloperProfile>
{
    public void Configure(EntityTypeBuilder<DeveloperProfile> builder)
    {
        builder.ToTable("DeveloperProfiles", DbSchemas.Identity);
        builder.HasKey(p => new { p.Id, p.UserId });
        builder.HasAlternateKey(p => p.Id);
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.Property(p => p.ProfileImage).HasMaxLength(500);
        builder.Property(p => p.Bio).HasMaxLength(500);
        builder.Property(p => p.AverageRating).HasColumnType("decimal(3,2)").HasDefaultValue(0m);
        builder.Property(p => p.RatingCount).HasDefaultValue(0);
    }
}
