using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class ClientNotificationSettingsRepository:GenericRepository<ClientNotificationSettings>, IClientNotificationSettingsRepository
    {
        public ClientNotificationSettingsRepository(ApplicationDbContext context):base(context)
        {
            
        }
    }
}
