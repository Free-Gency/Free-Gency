using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamSkillConfiguration : IEntityTypeConfiguration<TeamSkill>
{
    public void Configure(EntityTypeBuilder<TeamSkill> builder)
    {
        builder.ToTable("TeamSkills", DbSchemas.Teams);
        builder.HasKey(ts => new { ts.Id, ts.TeamId, ts.SkillId });
        builder.HasIndex(ts => new { ts.TeamId, ts.SkillId }).IsUnique();

        builder.HasOne(ts => ts.Team)
            .WithMany(t => t.TeamSkills)
            .HasForeignKey(ts => ts.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ts => ts.Skill)
            .WithMany(s => s.TeamSkills)
            .HasForeignKey(ts => ts.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
