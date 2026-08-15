
namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class MilestonePlanVersionConfiguration : IEntityTypeConfiguration<MilestonePlanVersion>
{
    public void Configure(EntityTypeBuilder<MilestonePlanVersion> builder)
    {
        builder.ToTable("MilestonePlanVersions", DbSchemas.Marketplace);
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PlanVersionStatus.Proposed);
        builder.Property(v => v.ChangeComment).HasMaxLength(4000);

        builder.HasIndex(v => new { v.ProjectId, v.Version }).IsUnique();

        builder.HasOne(v => v.Project)
            .WithMany(p => p.MilestonePlanVersions)
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Proposal)
            .WithMany(p => p.MilestonePlanVersions)
            .HasForeignKey(v => v.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
