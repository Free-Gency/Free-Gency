using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.AI;

public class HirePyInterviewConfiguration : IEntityTypeConfiguration<HirePyInterview>
{
    public void Configure(EntityTypeBuilder<HirePyInterview> builder)
    {
        builder.ToTable("HirePyInterviews", DbSchemas.AI);
        builder.HasKey(i => i.Id);

        builder.Property(i => i.HirePySessionId).IsRequired();
        builder.Property(i => i.ProjectId).IsRequired();
        builder.Property(i => i.ProjectProposalId).IsRequired();
        builder.Property(i => i.ChatRoomId).IsRequired();
        builder.Property(i => i.FreelancerUserId).IsRequired();
        builder.Property(i => i.FreelancerDeveloperProfileId).IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(HirePyInterviewStatus.Started);
        builder.Property(i => i.PlanningRounds).HasDefaultValue(0);
        builder.Property(i => i.FailReason).HasMaxLength(2000);
        builder.Property(i => i.ProcessingAt).HasColumnType("datetime2");
        builder.Property(i => i.ConcludedAt).HasColumnType("datetime2");

        builder.HasIndex(i => i.ProjectProposalId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(i => i.ChatRoomId);
        builder.HasIndex(i => i.Status);
    }
}
