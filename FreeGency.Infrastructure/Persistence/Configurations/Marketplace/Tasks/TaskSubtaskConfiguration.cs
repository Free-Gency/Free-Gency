namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace.Tasks;

public class TaskSubtaskConfiguration : IEntityTypeConfiguration<TaskSubtask>
{
    public void Configure(EntityTypeBuilder<TaskSubtask> builder)
    {
        builder.ToTable("TaskSubtasks", DbSchemas.Marketplace);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(300);
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.TaskStatus.Todo);
        builder.Property(s => s.DueDate).HasColumnType("datetime2");

        builder.HasOne(s => s.Task)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(s => s.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
