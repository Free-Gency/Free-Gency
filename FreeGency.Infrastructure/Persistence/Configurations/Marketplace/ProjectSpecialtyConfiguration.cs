using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ProjectSpecialtyConfiguration : IEntityTypeConfiguration<ProjectSpecialty>
{
    public void Configure(EntityTypeBuilder<ProjectSpecialty> builder)
    {
        builder.ToTable("ProjectSpecialties", DbSchemas.Marketplace);
        builder.HasKey(ps => new { ps.Id, ps.ProjectId, ps.SpecialtyId });
        builder.HasIndex(ps => new { ps.ProjectId, ps.SpecialtyId }).IsUnique();

        builder.HasOne(ps => ps.Project)
            .WithMany(p => p.ProjectSpecialties)
            .HasForeignKey(ps => ps.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Specialty)
            .WithMany(s => s.ProjectSpecialties)
            .HasForeignKey(ps => ps.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

