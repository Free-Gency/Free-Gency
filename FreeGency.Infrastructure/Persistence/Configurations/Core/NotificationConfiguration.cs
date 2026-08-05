using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Core;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", DbSchemas.Core, t =>
        {
            t.HasCheckConstraint(
                "CK_Notifications_ProfileScope",
                "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");
        });

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired();
        builder.Property(n => n.Body).IsRequired();

        builder.HasIndex(n => n.ClientProfileId);
        builder.HasIndex(n => n.DeveloperProfileId);
        builder.HasIndex(n => new { n.ClientProfileId, n.IsRead });
        builder.HasIndex(n => new { n.DeveloperProfileId, n.IsRead });

        builder.HasOne(n => n.ClientProfile)
            .WithMany(cp => cp.Notifications)
            .HasForeignKey(n => n.ClientProfileId)
            .HasPrincipalKey(cp => cp.Id)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(n => n.DeveloperProfile)
            .WithMany(dp => dp.Notifications)
            .HasForeignKey(n => n.DeveloperProfileId)
            .HasPrincipalKey(dp => dp.Id)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(n => n.Project)
            .WithMany()
            .HasForeignKey(n => n.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.ProjectProposal)
            .WithMany()
            .HasForeignKey(n => n.ProjectProposalId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.Team)
            .WithMany()
            .HasForeignKey(n => n.TeamId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.Milestone)
            .WithMany()
            .HasForeignKey(n => n.MilestoneId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.ChatRoom)
            .WithMany()
            .HasForeignKey(n => n.ChatRoomId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.Message)
            .WithMany()
            .HasForeignKey(n => n.MessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
