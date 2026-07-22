using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("UserInterests", DbSchemas.Identity, t =>
        {
            t.HasCheckConstraint(
                "CK_UserInterests_ProfileScope",
                "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");
        });

        builder.HasKey(ui => ui.Id);

        builder.HasIndex(ui => new { ui.ClientProfileId, ui.CategoryId })
            .IsUnique()
            .HasFilter("[ClientProfileId] IS NOT NULL");

        builder.HasIndex(ui => new { ui.DeveloperProfileId, ui.CategoryId })
            .IsUnique()
            .HasFilter("[DeveloperProfileId] IS NOT NULL");

        builder.HasOne(ui => ui.ClientProfile)
            .WithMany(cp => cp.UserInterests)
            .HasForeignKey(ui => ui.ClientProfileId)
            .HasPrincipalKey(cp => cp.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.DeveloperProfile)
            .WithMany(dp => dp.UserInterests)
            .HasForeignKey(ui => ui.DeveloperProfileId)
            .HasPrincipalKey(dp => dp.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Category)
            .WithMany(c => c.UserInterests)
            .HasForeignKey(ui => ui.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
