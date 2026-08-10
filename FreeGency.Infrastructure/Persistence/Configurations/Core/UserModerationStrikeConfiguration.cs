using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Core;

public class UserModerationStrikeConfiguration : IEntityTypeConfiguration<UserModerationStrike>
{
    public void Configure(EntityTypeBuilder<UserModerationStrike> builder)
    {
        builder.ToTable("UserModerationStrikes", DbSchemas.Core);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PrimaryCategory).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasOne(x => x.User)
            .WithMany(u => u.ModerationStrikes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ModerationCase)
            .WithMany()
            .HasForeignKey(x => x.ModerationCaseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
