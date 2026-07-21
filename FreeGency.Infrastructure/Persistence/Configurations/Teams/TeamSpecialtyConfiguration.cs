using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.TeamModule;

public class TeamSpecialtyConfiguration : IEntityTypeConfiguration<TeamSpecialty>
{
    public void Configure(EntityTypeBuilder<TeamSpecialty> builder)
    {
        builder.ToTable("TeamSpecialties", DbSchemas.Teams);
        builder.HasKey(ts => new { ts.Id, ts.TeamId, ts.SpecialtyId });
        builder.HasIndex(ts => new { ts.TeamId, ts.SpecialtyId }).IsUnique();

        builder.HasOne(ts => ts.Team)
            .WithMany(t => t.TeamSpecialties)
            .HasForeignKey(ts => ts.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ts => ts.Specialty)
            .WithMany(s => s.TeamSpecialties)
            .HasForeignKey(ts => ts.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

