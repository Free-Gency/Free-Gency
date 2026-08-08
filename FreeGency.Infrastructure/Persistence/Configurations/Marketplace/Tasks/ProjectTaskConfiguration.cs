namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace.Tasks;


public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("ProjectTasks", DbSchemas.Marketplace);
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.Requirements).HasMaxLength(4000);
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(TaskPriority.Medium);
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.TaskStatus.Todo);
        builder.Property(t => t.DueDate).HasColumnType("datetime2");
        builder.Property(t => t.EstimatedHours).HasColumnType("decimal(10,2)");
        builder.Property(t => t.SpentHours).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        builder.Property(t => t.CompletedAt).HasColumnType("datetime2");

        builder.HasIndex(t => t.MilestoneId);
        builder.HasIndex(t => t.AssigneeUserId);

        builder.HasOne(t => t.Milestone)
            .WithMany(m => m.Tasks)
            .HasForeignKey(t => t.MilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
