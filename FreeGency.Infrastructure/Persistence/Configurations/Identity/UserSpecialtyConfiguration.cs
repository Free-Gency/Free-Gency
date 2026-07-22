using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class UserSpecialtyConfiguration : IEntityTypeConfiguration<UserSpecialty>
{
    public void Configure(EntityTypeBuilder<UserSpecialty> builder)
    {
        builder.ToTable("UserSpecialties", DbSchemas.Identity, t =>
        {
            t.HasCheckConstraint(
                "CK_UserSpecialties_ProfileScope",
                "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");
        });

        builder.HasKey(us => us.Id);

        builder.HasIndex(us => new { us.ClientProfileId, us.SpecialtyId })
            .IsUnique()
            .HasFilter("[ClientProfileId] IS NOT NULL");

        builder.HasIndex(us => new { us.DeveloperProfileId, us.SpecialtyId })
            .IsUnique()
            .HasFilter("[DeveloperProfileId] IS NOT NULL");

        builder.HasOne(us => us.ClientProfile)
            .WithMany(cp => cp.UserSpecialties)
            .HasForeignKey(us => us.ClientProfileId)
            .HasPrincipalKey(cp => cp.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.DeveloperProfile)
            .WithMany(dp => dp.UserSpecialties)
            .HasForeignKey(us => us.DeveloperProfileId)
            .HasPrincipalKey(dp => dp.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.Specialty)
            .WithMany(s => s.UserSpecialties)
            .HasForeignKey(us => us.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
