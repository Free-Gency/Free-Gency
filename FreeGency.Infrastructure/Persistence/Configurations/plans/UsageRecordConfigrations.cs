using FreeGency.Domain.Entities.Plans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Configurations.plans
{
    public class UsageRecordConfigrations : IEntityTypeConfiguration<UsageRecord>
    {
        public void Configure(EntityTypeBuilder<UsageRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Feature)
                .IsRequired();

            builder.Property(x => x.Used)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.PeriodStart)
                .IsRequired();

            builder.Property(x => x.PeriodEnd)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.SubscriptionId,
                x.Feature
            })
            .IsUnique();    

            builder.HasOne(x => x.subscription)
                .WithMany()
                .HasForeignKey(x => x.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
