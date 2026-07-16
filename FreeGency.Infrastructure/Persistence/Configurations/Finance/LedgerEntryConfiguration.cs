using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Finance;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries", DbSchemas.Finance);
        builder.HasKey(l => l.Id);

        builder.Property(l => l.EntryType).HasConversion<string>().HasMaxLength(50);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.HasIndex(l => l.IdempotencyKey).IsUnique();
        builder.Property(l => l.PaymentProviderRef).HasMaxLength(200);

        builder.HasOne(l => l.Wallet)
            .WithMany(w => w.LedgerEntries)
            .HasForeignKey(l => l.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Project)
            .WithMany()
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(l => l.Milestone)
            .WithMany(m => m.LedgerEntries)
            .HasForeignKey(l => l.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
