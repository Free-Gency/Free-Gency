using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Catalog;

public class SpecialtySkillConfiguration : IEntityTypeConfiguration<SpecialtySkill>
{
    public void Configure(EntityTypeBuilder<SpecialtySkill> builder)
    {
        builder.ToTable("SpecialtySkills", DbSchemas.Catalog);
        builder.HasKey(ss => new { ss.Id, ss.SpecialtyId, ss.SkillId });
        builder.HasIndex(ss => new { ss.SpecialtyId, ss.SkillId }).IsUnique();

        builder.HasOne(ss => ss.Specialty)
            .WithMany(s => s.SpecialtySkills)
            .HasForeignKey(ss => ss.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.Skill)
            .WithMany(s => s.SpecialtySkills)
            .HasForeignKey(ss => ss.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

