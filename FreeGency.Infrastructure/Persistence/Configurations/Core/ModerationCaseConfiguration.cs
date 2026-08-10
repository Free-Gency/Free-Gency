using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Core;

public class ModerationCaseConfiguration : IEntityTypeConfiguration<ModerationCase>
{
    public void Configure(EntityTypeBuilder<ModerationCase> builder)
    {
        builder.ToTable("ModerationCases", DbSchemas.Core);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(ModerationCaseStatus.AutoResolved);
        builder.Property(x => x.ContentSnapshot).IsRequired();
        builder.Property(x => x.Categories).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UserMessage).HasMaxLength(500);
        builder.Property(x => x.AdminSummary).HasMaxLength(1000);
        builder.Property(x => x.AdminNote).HasMaxLength(2000);

        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.SourceType, x.SourceId });
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany(u => u.ModerationCases)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
