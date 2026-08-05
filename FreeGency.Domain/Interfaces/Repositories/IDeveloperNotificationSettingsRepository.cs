using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IDeveloperNotificationSettingsRepository:IGenericRepository<DeveloperNotificationSettings>
    {
        Task<DeveloperNotificationSettings?> GetDeveloperNotification(Guid profileId);
    }
}
