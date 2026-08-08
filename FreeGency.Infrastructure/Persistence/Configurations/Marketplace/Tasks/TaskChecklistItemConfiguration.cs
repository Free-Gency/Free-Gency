namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace.Tasks;

public class TaskChecklistItemConfiguration : IEntityTypeConfiguration<TaskChecklistItem>
{
    public void Configure(EntityTypeBuilder<TaskChecklistItem> builder)
    {
        builder.ToTable("TaskChecklistItems", DbSchemas.Marketplace);
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title).IsRequired().HasMaxLength(300);
        builder.Property(i => i.IsCompleted).HasDefaultValue(false);

        builder.HasOne(i => i.Task)
            .WithMany(t => t.ChecklistItems)
            .HasForeignKey(i => i.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
