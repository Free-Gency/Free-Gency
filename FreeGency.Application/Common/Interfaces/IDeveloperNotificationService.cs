using FreeGency.Application.Features.ClientNotification.Dtos;
using FreeGency.Application.Features.DeveloperNotification.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IDeveloperNotificationService
    {
        Task<Result<NotficationDeveloperDto>> GetNotifiactionDeveloperSettingAsync();
        Task<Result> UpdateNotificationDeveloperSettingAsync(UpdateNotificationDeveloperDto dto);
    }
}
