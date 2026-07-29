using FreeGency.Application.Features.ClientNotification.Dtos;
using FreeGency.Application.Features.ClientNotification.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ClientNotification.Commands
{
    public partial class ClientNotficationService
    {
        public async Task<Result> UpdateNotificationClientSettingAsync(UpdateNotificationClientDto dto)
        {
            var setting = await clientNotificationSettingsRepository.GetByIdAsync(dto.Id);
            if (setting == null) return Result.Failure(NotificationSettingError.SettingNotFound);
            setting.UpdateEntity(dto);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
