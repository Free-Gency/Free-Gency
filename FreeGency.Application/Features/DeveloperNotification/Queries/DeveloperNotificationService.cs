using FreeGency.Application.Features.DeveloperNotification.Dtos;
using FreeGency.Application.Features.DeveloperNotification.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.DeveloperNotification.Commands
{
    public partial class DeveloperNotificationService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
    {
        private readonly IDeveloperProfileRepository developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        private readonly IDeveloperNotificationSettingsRepository developerNotificationSettingsRepository = unitOfWork.Repository<IDeveloperNotificationSettingsRepository, DeveloperNotificationSettings>();
        public async Task<Result<NotficationDeveloperDto>> GetNotifiactionDeveloperSettingAsync()
        {
            var userId = currentUserService.UserId;
            var developerProfile = await developerProfileRepository.GetByUserIdAsync(userId);
            if (developerProfile == null) return Result.Failure<NotficationDeveloperDto>(ProfileErrors.DeveloperProfileNotFound);
            var developerNotification = await developerNotificationSettingsRepository.GetDeveloperNotification(developerProfile.Id);
            if (developerNotification == null) return Result.Failure<NotficationDeveloperDto>(NotificationErrors.SettingNotificationNotFound);
            var dto = developerNotification.ToDto();
            return Result.Success(dto);
        }
    }
}
