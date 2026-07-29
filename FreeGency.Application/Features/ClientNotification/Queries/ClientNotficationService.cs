using FreeGency.Application.Features.ClientNotification.Dtos;
using FreeGency.Application.Features.ClientNotification.Mapping;
using FreeGency.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ClientNotification.Commands
{
    public partial class ClientNotficationService(ICurrentUserService currentUserService,IUnitOfWork unitOfWork) : IClientNotficationService
    {
        private readonly IClientProfileRepository clientProfileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        private readonly IClientNotificationSettingsRepository clientNotificationSettingsRepository = unitOfWork.Repository<IClientNotificationSettingsRepository, ClientNotificationSettings>();
        public async Task<Result<NotficationClientDto>> GetNotifiactionCLientSettingAsync()
        {
            var userId = currentUserService.UserId;
            var clientProfile = await clientProfileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId, false));
            if (clientProfile == null) return Result.Failure<NotficationClientDto>(UserErrors.UserNotFound);
            var notficationSetting = await clientNotificationSettingsRepository.GetEntityWithSpec(new ClientNotificationSettingsSpecification(clientProfile.Id));
            if (notficationSetting == null) return Result.Failure<NotficationClientDto>(NotificationSettingError.SettingNotFound);
            var response = notficationSetting.ToDto();
            return Result.Success(response);
        }

       
    }
}
