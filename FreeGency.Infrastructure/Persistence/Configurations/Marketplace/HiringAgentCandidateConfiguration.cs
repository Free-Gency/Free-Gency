using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class HiringAgentCandidateConfiguration : IEntityTypeConfiguration<HiringAgentCandidate>
{
    public void Configure(EntityTypeBuilder<HiringAgentCandidate> builder)
    {
        builder.ToTable("HiringAgentCandidates", DbSchemas.Marketplace);
        builder.HasKey(c => c.Id);

        builder.Property(c => c.InviteeType).HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(HiringAgentCandidateStatus.Suggested);
        builder.Property(c => c.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.AvatarUrl).HasMaxLength(500);
        builder.Property(c => c.DiscussionNotes).HasMaxLength(4000);
        builder.Property(c => c.LastAgentMessageAt).HasColumnType("datetime2");

        builder.HasIndex(c => c.HiringAgentRunId);
        builder.HasIndex(c => c.InvitationId);
        builder.HasIndex(c => c.ProposalId);
        builder.HasIndex(c => c.ChatRoomId);
        builder.HasIndex(c => c.Status);

        // NoAction: SQL Server rejects multiple cascade/SetNull paths through marketplace/chat graphs.
        builder.HasOne(c => c.Invitation)
            .WithMany()
            .HasForeignKey(c => c.InvitationId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(c => c.Proposal)
            .WithMany()
            .HasForeignKey(c => c.ProposalId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(c => c.ChatRoom)
            .WithMany()
            .HasForeignKey(c => c.ChatRoomId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(c => c.LatestPlanVersion)
            .WithMany()
            .HasForeignKey(c => c.LatestPlanVersionId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_HiringAgentCandidates_InviteeScope",
            "(InviteeType = 'User' AND InviteeUserId IS NOT NULL AND InviteeTeamId IS NULL) OR " +
            "(InviteeType = 'Team' AND InviteeTeamId IS NOT NULL AND InviteeUserId IS NULL)"));
    }
}
