using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IClientNotificationSettingsRepository:IGenericRepository<ClientNotificationSettings>
    {
        Task<ClientNotificationSettings> GetByProfileId(Guid profileId);
    }
}
