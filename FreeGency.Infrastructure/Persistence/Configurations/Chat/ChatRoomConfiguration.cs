using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Chat;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms", DbSchemas.Chat);
        builder.HasKey(c => c.Id);

        builder.Property(c => c.RoomType).HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ChatRoomStatus.Active);
        builder.Property(c => c.ArchivedAt).HasColumnType("datetime2");
        builder.Property(c => c.Title).HasMaxLength(200);

        builder.HasIndex(c => c.TeamId)
            .IsUnique()
            .HasFilter("[RoomType] = 'TeamMain'");

        builder.HasIndex(c => c.ProjectId)
            .IsUnique()
            .HasFilter("[ProjectId] IS NOT NULL AND [ProjectId] <> '00000000-0000-0000-0000-000000000000'");

        builder.HasIndex(c => c.ProposalId)
            .IsUnique()
            .HasFilter("[ProposalId] IS NOT NULL AND [ProposalId] <> '00000000-0000-0000-0000-000000000000'");

        builder.HasOne(c => c.Team)
            .WithMany(t => t.ChatRooms)
            .HasForeignKey(c => c.TeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(c => c.Project)
            .WithMany(p => p.ChatRooms)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(c => c.Proposal)
            .WithOne(p => p.ChatRoom)
            .HasForeignKey<ChatRoom>(c => c.ProposalId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany(u => u.CreatedChatRooms)
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(c => c.SourceProposalRoom)
            .WithMany()
            .HasForeignKey(c => c.SourceProposalRoomId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
