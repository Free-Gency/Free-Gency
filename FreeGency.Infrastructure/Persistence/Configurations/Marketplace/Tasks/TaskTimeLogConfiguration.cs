namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace.Tasks;

public class TaskTimeLogConfiguration : IEntityTypeConfiguration<TaskTimeLog>
{
    public void Configure(EntityTypeBuilder<TaskTimeLog> builder)
    {
        builder.ToTable("TaskTimeLogs", DbSchemas.Marketplace);
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Hours).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(l => l.Note).HasMaxLength(1000);
        builder.Property(l => l.WorkDate).HasColumnType("datetime2").IsRequired();

        builder.HasOne(l => l.Task)
            .WithMany(t => t.TimeLogs)
            .HasForeignKey(l => l.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
