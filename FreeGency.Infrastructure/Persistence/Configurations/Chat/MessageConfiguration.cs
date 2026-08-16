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
        builder.ToTable("Messages", DbSchemas.Chat, t =>
        {
            t.HasCheckConstraint(
                "CK_Messages_SenderProfileScope",
                "(SenderClientProfileId IS NULL AND SenderDeveloperProfileId IS NULL) OR " +
                "(SenderClientProfileId IS NOT NULL AND SenderDeveloperProfileId IS NULL) OR " +
                "(SenderClientProfileId IS NULL AND SenderDeveloperProfileId IS NOT NULL)");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MessageType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(MessageType.Text);
        builder.Property(m => m.Text);
        builder.Property(m => m.FileUrl).HasMaxLength(500);
        builder.Property(m => m.FileName).HasMaxLength(255);
        builder.Property(m => m.ModerationStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(ModerationStatus.Visible);
        builder.Property(m => m.ModerationNote).HasMaxLength(500);
        builder.Property(m => m.ModeratedText);
        builder.Property(m => m.IsAgentGenerated).HasDefaultValue(false);

        builder.HasIndex(m => m.SenderClientProfileId);
        builder.HasIndex(m => m.SenderDeveloperProfileId);
        builder.HasIndex(m => new { m.ChatRoomId, m.CreatedAt });

        builder.HasOne(m => m.ChatRoom)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.SenderClientProfile)
            .WithMany(cp => cp.SentMessages)
            .HasForeignKey(m => m.SenderClientProfileId)
            .HasPrincipalKey(cp => cp.Id)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(m => m.SenderDeveloperProfile)
            .WithMany(dp => dp.SentMessages)
            .HasForeignKey(m => m.SenderDeveloperProfileId)
            .HasPrincipalKey(dp => dp.Id)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(m => m.PlanVersion)
            .WithMany()
            .HasForeignKey(m => m.PlanVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(m => m.Milestone)
            .WithMany()
            .HasForeignKey(m => m.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(m => m.MilestoneId);
    }
}
