using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class DeveloperNotificationSettingsRepository:GenericRepository<DeveloperNotificationSettings>, IDeveloperNotificationSettingsRepository
    {
        public DeveloperNotificationSettingsRepository(ApplicationDbContext context):base(context)
        {
            
        }

        public async Task<DeveloperNotificationSettings?> GetDeveloperNotification(Guid profileId)
        {
            return await _dbSet.Where(x => x.ProfileId == profileId).FirstOrDefaultAsync();
        }

      
    }
}
