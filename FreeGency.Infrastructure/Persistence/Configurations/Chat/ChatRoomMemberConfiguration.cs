using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Chat;

public class ChatRoomMemberConfiguration : IEntityTypeConfiguration<ChatRoomMember>
{
    public void Configure(EntityTypeBuilder<ChatRoomMember> builder)
    {
        builder.ToTable("ChatRoomMembers", DbSchemas.Chat);
        builder.HasKey(m => new { m.Id, m.ChatRoomId, m.UserId });
        builder.HasIndex(m => new { m.ChatRoomId, m.UserId }).IsUnique();

        builder.Property(m => m.JoinedAt).HasColumnType("datetime2");
        builder.Property(m => m.LastReadAt).HasColumnType("datetime2");
        builder.Property(m => m.RoleLabel).HasMaxLength(100);
        builder.Property(m => m.CanSend).HasDefaultValue(true);

        builder.HasOne(m => m.ChatRoom)
            .WithMany(c => c.ChatRoomMembers)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany(u => u.ChatRoomMembers)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
