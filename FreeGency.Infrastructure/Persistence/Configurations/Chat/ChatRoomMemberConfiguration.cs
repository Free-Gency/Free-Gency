using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Chat;

public class ChatRoomMemberConfiguration : IEntityTypeConfiguration<ChatRoomMember>
{
    public void Configure(EntityTypeBuilder<ChatRoomMember> builder)
    {
        builder.ToTable("ChatRoomMembers", DbSchemas.Chat, t =>
        {
            t.HasCheckConstraint(
                "CK_ChatRoomMembers_ProfileScope",
                "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");
        });

        builder.HasKey(m => m.Id);

        builder.HasIndex(m => new { m.ChatRoomId, m.ClientProfileId })
            .IsUnique()
            .HasFilter("[ClientProfileId] IS NOT NULL");

        builder.HasIndex(m => new { m.ChatRoomId, m.DeveloperProfileId })
            .IsUnique()
            .HasFilter("[DeveloperProfileId] IS NOT NULL");

        builder.Property(m => m.JoinedAt).HasColumnType("datetime2");
        builder.Property(m => m.LastReadAt).HasColumnType("datetime2");
        builder.Property(m => m.RoleLabel).HasMaxLength(100);
        builder.Property(m => m.CanSend).HasDefaultValue(true);

        builder.HasOne(m => m.ChatRoom)
            .WithMany(c => c.ChatRoomMembers)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ClientProfile)
            .WithMany(cp => cp.ChatRoomMembers)
            .HasForeignKey(m => m.ClientProfileId)
            .HasPrincipalKey(cp => cp.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.DeveloperProfile)
            .WithMany(dp => dp.ChatRoomMembers)
            .HasForeignKey(m => m.DeveloperProfileId)
            .HasPrincipalKey(dp => dp.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
