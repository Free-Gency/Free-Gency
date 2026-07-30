using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class MilestonePlanItemConfiguration : IEntityTypeConfiguration<MilestonePlanItem>
{
    public void Configure(EntityTypeBuilder<MilestonePlanItem> builder)
    {
        builder.ToTable("MilestonePlanItems", DbSchemas.Marketplace);
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
        builder.Property(i => i.DefinitionOfDone).IsRequired();
        builder.Property(i => i.Amount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DueDate).HasColumnType("datetime2");
        builder.Property(i => i.ChangeTag).HasMaxLength(20);

        builder.HasOne(i => i.PlanVersion)
            .WithMany(v => v.Items)
            .HasForeignKey(i => i.PlanVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
