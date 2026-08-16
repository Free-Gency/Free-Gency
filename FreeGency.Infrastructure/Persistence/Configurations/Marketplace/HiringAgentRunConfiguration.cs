using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class HiringAgentRunConfiguration : IEntityTypeConfiguration<HiringAgentRun>
{
    public void Configure(EntityTypeBuilder<HiringAgentRun> builder)
    {
        builder.ToTable("HiringAgentRuns", DbSchemas.Marketplace);
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(HiringAgentRunStatus.Queued);
        builder.Property(r => r.TopK).HasDefaultValue(5);
        builder.Property(r => r.InviteDeadlineUtc).HasColumnType("datetime2");
        builder.Property(r => r.DiscussionDeadlineUtc).HasColumnType("datetime2");
        builder.Property(r => r.ReportReadyAt).HasColumnType("datetime2");
        builder.Property(r => r.CompletedAt).HasColumnType("datetime2");
        builder.Property(r => r.ClientHireApprovedAt).HasColumnType("datetime2");
        builder.Property(r => r.ReportJson);
        builder.Property(r => r.FailureReason).HasMaxLength(2000);

        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => r.ClientUserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.ProjectId, r.Status });

        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ClientUser)
            .WithMany()
            .HasForeignKey(r => r.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RecommendedProposal)
            .WithMany()
            .HasForeignKey(r => r.RecommendedProposalId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(r => r.RecommendedPlanVersion)
            .WithMany()
            .HasForeignKey(r => r.RecommendedPlanVersionId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(r => r.RecommendedCandidate)
            .WithMany()
            .HasForeignKey(r => r.RecommendedCandidateId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasMany(r => r.Candidates)
            .WithOne(c => c.HiringAgentRun)
            .HasForeignKey(c => c.HiringAgentRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
