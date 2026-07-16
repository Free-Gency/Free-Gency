using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
{
    public void Configure(EntityTypeBuilder<ProjectFile> builder)
    {
        builder.ToTable("ProjectFiles", DbSchemas.Marketplace);
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName).IsRequired().HasMaxLength(255);
        builder.Property(f => f.FileUrl).IsRequired().HasMaxLength(500);
        builder.Property(f => f.FileKind).HasConversion<string>().HasMaxLength(50);

        builder.HasOne(f => f.Project)
            .WithMany(p => p.ProjectFiles)
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Milestone)
            .WithMany(m => m.ProjectFiles)
            .HasForeignKey(f => f.MilestoneId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(f => f.UploadedByUser)
            .WithMany(u => u.UploadedProjectFiles)
            .HasForeignKey(f => f.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
