using FreeGency.Application.Features.DeveloperNotification.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.DeveloperNotification.Mapping
{
    public static class settingDeveloperNotificationMapping
    {
        public static NotficationDeveloperDto ToDto(this DeveloperNotificationSettings settings)
        {
            return new NotficationDeveloperDto
            {
                Id = settings.Id,
                WalletInApp = settings.WalletInApp,
                WalletEmail = settings.WalletEmail,
                ProjectsInApp = settings.ProjectsInApp,
                ProjectsEmail = settings.ProjectsEmail,
                MessagesInApp = settings.MessagesInApp,
                MessagesEmail = settings.MessagesEmail,
                TeamsInApp = settings.TeamsInApp,
                TeamsEmail = settings.TeamsEmail,
                MilestonesInApp = settings.MilestonesInApp,
                MilestonesEmail = settings.MilestonesEmail
            };
        }
        public static void UpdateEntity(this DeveloperNotificationSettings settings,UpdateNotificationDeveloperDto dto)
        {
            settings.WalletInApp = dto.WalletInApp;
            settings.WalletEmail = dto.WalletEmail;
            settings.ProjectsInApp = dto.ProjectsInApp;
            settings.ProjectsEmail = dto.ProjectsEmail;
            settings.MilestonesInApp = dto.MilestonesInApp;
            settings.MilestonesEmail = dto.MilestonesEmail;
            settings.TeamsEmail = dto.TeamsEmail;
            settings.TeamsInApp = dto.TeamsInApp;
            settings.MessagesInApp = dto.MessagesInApp;
            settings.MessagesEmail = dto.MessagesEmail;
        }
    }
}
