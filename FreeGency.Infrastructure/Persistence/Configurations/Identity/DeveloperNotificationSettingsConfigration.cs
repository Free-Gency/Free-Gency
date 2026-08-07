using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity
{
    public class DeveloperNotificationSettingsConfigration : IEntityTypeConfiguration<DeveloperNotificationSettings>
    {
        public void Configure(EntityTypeBuilder<DeveloperNotificationSettings> builder)
        {
            builder.HasOne(x => x.developerProfile)
                 .WithOne(x => x.DeveloperNotificationSettings)
                 .HasPrincipalKey<DeveloperProfile>(x => x.Id)
                 .HasForeignKey<DeveloperNotificationSettings>(x => x.ProfileId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
