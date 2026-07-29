using FreeGency.Application.Features.ClientNotification.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IClientNotficationService
    {
        Task<Result<NotficationClientDto>> GetNotifiactionCLientSettingAsync();
        Task<Result> UpdateNotificationClientSettingAsync(UpdateNotificationClientDto dto);
    }
}
