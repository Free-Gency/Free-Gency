

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", DbSchemas.Marketplace);
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.Property(p => p.BudgetMin).HasColumnType("decimal(18,2)");
        builder.Property(p => p.BudgetMax).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ProjectStatus.Draft);
        builder.Property(p => p.Deadline).HasColumnType("datetime2");
        builder.Property(p => p.CompletedAt).HasColumnType("datetime2");
        builder.Property(p => p.AssignedUserId).HasMaxLength(256);

        builder.HasOne(p => p.Client)
            .WithMany(u => u.PostedProjects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AssignedTeam)
            .WithMany(t => t.AssignedProjects)
            .HasForeignKey(p => p.AssignedTeamId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
