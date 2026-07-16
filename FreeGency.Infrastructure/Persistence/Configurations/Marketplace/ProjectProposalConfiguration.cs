using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ProjectProposalConfiguration : IEntityTypeConfiguration<ProjectProposal>
{
    public void Configure(EntityTypeBuilder<ProjectProposal> builder)
    {
        builder.ToTable("ProjectProposals", DbSchemas.Marketplace);
        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.ApplicantType).HasConversion<string>().HasMaxLength(50);
        builder.Property(pp => pp.CoverLetter).IsRequired();
        builder.Property(pp => pp.ProposedBudget).HasColumnType("decimal(18,2)");
        builder.Property(pp => pp.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ProposalStatus.Pending);
        builder.Property(pp => pp.AppliedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(pp => pp.ResponseAt).HasColumnType("datetime2");

        builder.HasIndex(pp => new { pp.ProjectId, pp.TeamId })
            .IsUnique()
            .HasFilter("[TeamId] IS NOT NULL AND [TeamId] <> '00000000-0000-0000-0000-000000000000'");

        builder.HasIndex(pp => new { pp.ProjectId, pp.UserId })
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL AND [UserId] <> '00000000-0000-0000-0000-000000000000'");

        builder.HasOne(pp => pp.Project)
            .WithMany(p => p.ProjectProposals)
            .HasForeignKey(pp => pp.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Team)
            .WithMany(t => t.ProjectProposals)
            .HasForeignKey(pp => pp.TeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(pp => pp.User)
            .WithMany(u => u.ProjectProposals)
            .HasForeignKey(pp => pp.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(pp => pp.ChatRoom)
            .WithOne(c => c.Proposal)
            .HasForeignKey<ChatRoom>(c => c.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
