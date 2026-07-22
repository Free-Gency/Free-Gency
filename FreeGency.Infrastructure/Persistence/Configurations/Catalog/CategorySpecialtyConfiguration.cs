using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Catalog;

public class CategorySpecialtyConfiguration : IEntityTypeConfiguration<CategorySpecialty>
{
    public void Configure(EntityTypeBuilder<CategorySpecialty> builder)
    {
        builder.ToTable("CategorySpecialties", DbSchemas.Catalog);
        builder.HasKey(cs => new { cs.Id, cs.CategoryId, cs.SpecialtyId });
        builder.HasIndex(cs => new { cs.CategoryId, cs.SpecialtyId }).IsUnique();

        builder.HasOne(cs => cs.Category)
            .WithMany(c => c.CategorySpecialties)
            .HasForeignKey(cs => cs.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Specialty)
            .WithMany(s => s.CategorySpecialties)
            .HasForeignKey(cs => cs.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

