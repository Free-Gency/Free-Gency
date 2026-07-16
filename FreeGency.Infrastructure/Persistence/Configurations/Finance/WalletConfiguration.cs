using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Finance;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets", DbSchemas.Finance);
        builder.HasKey(w => w.Id);

        builder.Property(w => w.OwnerType).HasConversion<string>().HasMaxLength(50);
        builder.Property(w => w.Currency).IsRequired().HasMaxLength(3);
        builder.Property(w => w.Available).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(w => w.Reserved).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(w => w.Pending).HasColumnType("decimal(18,2)").HasDefaultValue(0m);

        builder.HasIndex(w => new { w.OwnerType, w.OwnerId, w.Currency }).IsUnique();
    }
}
