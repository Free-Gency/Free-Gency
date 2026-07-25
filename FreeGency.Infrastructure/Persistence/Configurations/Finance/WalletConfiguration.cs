using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Finance;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets", DbSchemas.Finance, t =>
        {
            t.HasCheckConstraint(
                "CK_Wallets_OwnerScope",
                "(OwnerType = N'User' AND OwnerUserId IS NOT NULL AND OwnerTeamId IS NULL) OR (OwnerType = N'Team' AND OwnerTeamId IS NOT NULL AND OwnerUserId IS NULL)");
        });

        builder.HasKey(w => w.Id);

        builder.Property(w => w.OwnerType).HasConversion<string>().HasMaxLength(50);
        builder.Property(w => w.Currency).IsRequired().HasMaxLength(3);
        builder.Property(w => w.Available).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(w => w.Reserved).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(w => w.Pending).HasColumnType("decimal(18,2)").HasDefaultValue(0m);

        builder.HasOne(w => w.OwnerUser)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(w => w.OwnerTeam)
            .WithOne(t => t.Wallet)
            .HasForeignKey<Wallet>(w => w.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(w => w.OwnerUserId)
            .IsUnique()
            .HasFilter("[OwnerUserId] IS NOT NULL");

        builder.HasIndex(w => w.OwnerTeamId)
            .IsUnique()
            .HasFilter("[OwnerTeamId] IS NOT NULL");
    }
}
