
namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;


public class MilestoneAssignmentConfiguration : IEntityTypeConfiguration<MilestoneAssignment>
{
    public void Configure(EntityTypeBuilder<MilestoneAssignment> builder)
    {
        builder.ToTable("MilestoneAssignments", DbSchemas.Marketplace);
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.MilestoneId, a.UserId }).IsUnique();

        builder.Property(a => a.Percentage).HasPrecision(18, 2);
        builder.Property(a => a.AssignedAt).HasColumnType("datetime2");

        builder.HasOne(a => a.Milestone)
            .WithMany(m => m.Assignments)
            .HasForeignKey(a => a.MilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
