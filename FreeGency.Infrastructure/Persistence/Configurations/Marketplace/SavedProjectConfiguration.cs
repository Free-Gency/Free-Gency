using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class SavedProjectConfiguration : IEntityTypeConfiguration<SavedProject>
{
    public void Configure(EntityTypeBuilder<SavedProject> builder)
    {
        builder.ToTable("SavedProjects", DbSchemas.Marketplace);
        builder.HasKey(sp => new { sp.Id, sp.UserId, sp.ProjectId });
        builder.HasIndex(sp => new { sp.UserId, sp.ProjectId }).IsUnique();

        builder.HasOne(sp => sp.User)
            .WithMany(u => u.SavedProjects)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Project)
            .WithMany(p => p.SavedProjects)
            .HasForeignKey(sp => sp.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
