using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Core;

public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.ToTable("SocialLinks", DbSchemas.Core);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.OwnerType).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Platform).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Url).IsRequired().HasMaxLength(500);

        builder.HasOne(s => s.OwnerUser)
            .WithMany(u => u.SocialLinks)
            .HasForeignKey(s => s.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(s => s.OwnerTeam)
            .WithMany(t => t.SocialLinks)
            .HasForeignKey(s => s.OwnerTeamId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
