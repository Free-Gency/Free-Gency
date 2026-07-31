using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Chat;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages", DbSchemas.Chat);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MessageType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(MessageType.Text);
        builder.Property(m => m.Text);
        builder.Property(m => m.FileUrl).HasMaxLength(500);
        builder.Property(m => m.FileName).HasMaxLength(255);

        builder.HasOne(m => m.ChatRoom)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.SenderUser)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(m => m.PlanVersion)
            .WithMany()
            .HasForeignKey(m => m.PlanVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
