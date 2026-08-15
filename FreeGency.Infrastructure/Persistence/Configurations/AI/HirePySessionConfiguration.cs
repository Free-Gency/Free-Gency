using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.AI;

public class HirePySessionConfiguration : IEntityTypeConfiguration<HirePySession>
{
    public void Configure(EntityTypeBuilder<HirePySession> builder)
    {
        builder.ToTable("HirePySessions", DbSchemas.AI);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClientUserId).IsRequired();
        builder.Property(s => s.Description).IsRequired().HasMaxLength(5000);
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(HirePySessionStatus.Draft);
        builder.Property(s => s.FailReason).HasMaxLength(2000);

        builder.Property(s => s.Title).HasMaxLength(200);
        builder.Property(s => s.GeneratedDescription).HasMaxLength(5000);
        builder.Property(s => s.CategoryName).HasMaxLength(100);
        builder.Property(s => s.BudgetMin).HasColumnType("decimal(18,2)");
        builder.Property(s => s.BudgetMax).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Currency).HasMaxLength(3);
        builder.Property(s => s.Deadline).HasColumnType("datetime2");
        builder.Property(s => s.Complexity).HasMaxLength(20);

        builder.Property(s => s.SkillIdsJson).HasColumnType("nvarchar(max)");
        builder.Property(s => s.SpecialtyIdsJson).HasColumnType("nvarchar(max)");
        builder.Property(s => s.RequirementsJson).HasColumnType("nvarchar(max)");
        builder.Property(s => s.FeaturesJson).HasColumnType("nvarchar(max)");
        builder.Property(s => s.RisksJson).HasColumnType("nvarchar(max)");
        builder.Property(s => s.SelectedCandidatesJson).HasColumnType("nvarchar(max)");

        builder.Property(s => s.SelectedCandidateName).HasMaxLength(200);
        builder.Property(s => s.DecisionReason).HasMaxLength(4000);
        builder.Property(s => s.MilestoneSummary).HasMaxLength(4000);
        builder.Property(s => s.SelectedBudget).HasColumnType("decimal(18,2)");
        builder.Property(s => s.SelectedTimeline).HasMaxLength(500);
        builder.Property(s => s.RecommendationCompletedAt).HasColumnType("datetime2");

        builder.Property(s => s.AcceptedPlanVersionId).IsRequired(false);

        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
