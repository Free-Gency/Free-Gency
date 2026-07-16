using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Portfolio;

public class PortfolioSkillConfiguration : IEntityTypeConfiguration<PortfolioSkill>
{
    public void Configure(EntityTypeBuilder<PortfolioSkill> builder)
    {
        builder.ToTable("PortfolioSkills", DbSchemas.Portfolio);
        builder.HasKey(ps => new { ps.Id, ps.PortfolioProjectId, ps.SkillId });
        builder.HasIndex(ps => new { ps.PortfolioProjectId, ps.SkillId }).IsUnique();

        builder.HasOne(ps => ps.PortfolioProject)
            .WithMany(p => p.PortfolioSkills)
            .HasForeignKey(ps => ps.PortfolioProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Skill)
            .WithMany(s => s.PortfolioSkills)
            .HasForeignKey(ps => ps.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
