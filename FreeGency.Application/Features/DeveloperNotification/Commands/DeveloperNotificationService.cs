using FreeGency.Application.Features.DeveloperNotification.Dtos;
using FreeGency.Application.Features.DeveloperNotification.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.DeveloperNotification.Commands
{
    public partial class DeveloperNotificationService : IDeveloperNotificationService
    {


        public async Task<Result> UpdateNotificationDeveloperSettingAsync(UpdateNotificationDeveloperDto dto)
        {
            var setting = await developerNotificationSettingsRepository.GetByIdAsync(dto.Id);
            if (setting == null) return Result.Failure(NotificationSettingError.SettingNotFound);
            setting.UpdateEntity(dto);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
