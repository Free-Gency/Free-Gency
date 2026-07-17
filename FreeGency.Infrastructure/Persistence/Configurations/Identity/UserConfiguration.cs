using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", DbSchemas.Identity);

        builder.Property(u => u.FristName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.IsVerified).HasDefaultValue(false);
        builder.Property(u => u.ActiveProfileMode)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(false);

        builder.HasOne(u => u.ClientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<ClientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.DeveloperProfile)
            .WithOne(p => p.User)
            .HasForeignKey<DeveloperProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.OwnedTeams)
            .WithOne(t => t.Owner)
            .HasForeignKey(t => t.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.PostedProjects)
            .WithOne(p => p.Client)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.OwnsMany(x => x.refreshTokens)
                .ToTable("RefreshTokens")
                .WithOwner()
                .HasForeignKey("UserId");
    }
}
