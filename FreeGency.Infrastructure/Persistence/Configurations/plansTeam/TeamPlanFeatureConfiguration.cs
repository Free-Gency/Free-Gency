using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Infrastructure.Persistence.Seeding;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Configurations.plansTeam
{
    public class TeamPlanFeatureConfiguration
     : IEntityTypeConfiguration<TeamPlanFeature>
    {
        public void Configure(EntityTypeBuilder<TeamPlanFeature> builder)
        {
            builder.ToTable("TeamPlanFeatures", "TeamPlans");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Feature)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.Limit);

            builder.Property(x => x.IsEnabled)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.TeamPlanId,
                x.Feature
            })
            .IsUnique();
            builder.HasData(TeamPlanSeeds.Features);
        }
    }
}
