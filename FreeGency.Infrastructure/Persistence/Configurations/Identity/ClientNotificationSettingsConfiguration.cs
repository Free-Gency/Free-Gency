using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Configurations.Identity
{
    public class ClientNotificationSettingsConfiguration : IEntityTypeConfiguration<ClientNotificationSettings>
    {
        public void Configure(EntityTypeBuilder<ClientNotificationSettings> builder)
        {
            builder.HasOne(x => x.clientProfile)
                .WithOne(x => x.ClientNotificationSettings)
                .HasPrincipalKey<ClientProfile>(x => x.Id)
                .HasForeignKey<ClientNotificationSettings>(x => x.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
