using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreeGency.Infrastructure.Persistence.Configurations.Marketplace;

public class ProposalAttachmentConfiguration : IEntityTypeConfiguration<ProposalAttachment>
{
    public void Configure(EntityTypeBuilder<ProposalAttachment> builder)
    {
        builder.ToTable("ProposalAttachments", DbSchemas.Marketplace);
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(500);

        builder.HasOne(a => a.Proposal)
            .WithMany(p => p.ProposalAttachments)
            .HasForeignKey(a => a.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
