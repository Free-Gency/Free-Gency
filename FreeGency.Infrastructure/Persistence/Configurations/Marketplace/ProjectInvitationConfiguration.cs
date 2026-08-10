using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ProjectInvitationConfiguration : IEntityTypeConfiguration<ProjectInvitation>
{
    public void Configure(EntityTypeBuilder<ProjectInvitation> builder)
    {
        builder.ToTable("ProjectInvitations", DbSchemas.Marketplace);
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InviteeType).HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.Message).IsRequired().HasMaxLength(4000);
        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ProjectInvitationStatus.Pending);
        builder.Property(i => i.RespondedAt).HasColumnType("datetime2");

        builder.HasIndex(i => new { i.ProjectId, i.InviteeUserId })
            .IsUnique()
            .HasFilter("[InviteeType] = 'User' AND [Status] = 'Pending' AND [IsDeleted] = 0");

        builder.HasIndex(i => new { i.ProjectId, i.InviteeTeamId })
            .IsUnique()
            .HasFilter("[InviteeType] = 'Team' AND [Status] = 'Pending' AND [IsDeleted] = 0");

        builder.HasIndex(i => i.ClientUserId);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ClientUser)
            .WithMany()
            .HasForeignKey(i => i.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.InviteeUser)
            .WithMany()
            .HasForeignKey(i => i.InviteeUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(i => i.InviteeTeam)
            .WithMany()
            .HasForeignKey(i => i.InviteeTeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(i => i.Proposal)
            .WithMany()
            .HasForeignKey(i => i.ProposalId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(i => i.ChatRoom)
            .WithMany()
            .HasForeignKey(i => i.ChatRoomId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ProjectInvitations_InviteeScope",
            "(InviteeType = 'User' AND InviteeUserId IS NOT NULL AND InviteeTeamId IS NULL) OR " +
            "(InviteeType = 'Team' AND InviteeTeamId IS NOT NULL)"));
    }
}
