using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamJobSkillConfiguration : IEntityTypeConfiguration<TeamJobSkill>
{
    public void Configure(EntityTypeBuilder<TeamJobSkill> builder)
    {
        builder.ToTable("TeamJobSkills", DbSchemas.Teams);
        builder.HasKey(tjs => new { tjs.Id, tjs.TeamJobId, tjs.SkillId });
        builder.HasIndex(tjs => new { tjs.TeamJobId, tjs.SkillId }).IsUnique();

        builder.HasOne(tjs => tjs.TeamJob)
            .WithMany(tj => tj.TeamJobSkills)
            .HasForeignKey(tjs => tjs.TeamJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tjs => tjs.Skill)
            .WithMany(s => s.TeamJobSkills)
            .HasForeignKey(tjs => tjs.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
