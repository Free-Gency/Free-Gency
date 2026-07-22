using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity;

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("UserSkills", DbSchemas.Identity, t =>
        {
            t.HasCheckConstraint(
                "CK_UserSkills_ProfileScope",
                "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");
        });

        builder.HasKey(us => us.Id);

        builder.HasIndex(us => new { us.ClientProfileId, us.SkillId })
            .IsUnique()
            .HasFilter("[ClientProfileId] IS NOT NULL");

        builder.HasIndex(us => new { us.DeveloperProfileId, us.SkillId })
            .IsUnique()
            .HasFilter("[DeveloperProfileId] IS NOT NULL");

        builder.HasOne(us => us.ClientProfile)
            .WithMany(cp => cp.UserSkills)
            .HasForeignKey(us => us.ClientProfileId)
            .HasPrincipalKey(cp => cp.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.DeveloperProfile)
            .WithMany(dp => dp.UserSkills)
            .HasForeignKey(us => us.DeveloperProfileId)
            .HasPrincipalKey(dp => dp.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Skill)
            .WithMany(s => s.UserSkills)
            .HasForeignKey(us => us.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
