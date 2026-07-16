using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("UserInterests", DbSchemas.Identity);
        builder.HasKey(ui => new { ui.Id, ui.UserId, ui.CategoryId });
        builder.HasIndex(ui => new { ui.UserId, ui.CategoryId }).IsUnique();

        builder.HasOne(ui => ui.User)
            .WithMany(u => u.UserInterests)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Category)
            .WithMany(c => c.UserInterests)
            .HasForeignKey(ui => ui.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
