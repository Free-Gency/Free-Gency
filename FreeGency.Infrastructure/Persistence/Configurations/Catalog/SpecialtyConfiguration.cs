using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Catalog;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("Specialties", DbSchemas.Catalog);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(s => s.NameAr).IsRequired().HasMaxLength(100);

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Specialties)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
