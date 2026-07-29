using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class ClientNotificationSettingsSpecification:BaseSpecification<ClientNotificationSettings>
    {
        public ClientNotificationSettingsSpecification(Guid profileId):base(x=>x.ProfileId==profileId)
        {
            
        }
    }
}
