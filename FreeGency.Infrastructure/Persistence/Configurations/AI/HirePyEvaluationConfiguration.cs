using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.AI;

public class HirePyEvaluationConfiguration : IEntityTypeConfiguration<HirePyEvaluation>
{
    public void Configure(EntityTypeBuilder<HirePyEvaluation> builder)
    {
        builder.ToTable("HirePyEvaluations", DbSchemas.AI);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CandidateName).HasMaxLength(200);
        builder.Property(e => e.Reason).HasMaxLength(4000);
        builder.Property(e => e.MilestoneSummary).HasMaxLength(4000);
        builder.Property(e => e.ProposedBudget).HasColumnType("decimal(18,2)");
        builder.Property(e => e.ProposedTimeline).HasMaxLength(500);

        builder.Property(e => e.StrengthsJson).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ConcernsJson).HasColumnType("nvarchar(max)");
        builder.Property(e => e.RisksJson).HasColumnType("nvarchar(max)");

        builder.Property(e => e.EvaluatedAt).HasColumnType("datetime2");
        builder.HasIndex(e => e.HirePySessionId);
    }
}
