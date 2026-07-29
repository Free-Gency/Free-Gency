using FreeGency.Application.Features.ClientNotification.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ClientNotification.Mapping
{
    public static class ClientNotificationMapping
    {
        public static NotficationClientDto ToDto(this ClientNotificationSettings settings)
        {
            return new NotficationClientDto
            {
                Id=settings.Id,
                NewMessageEmail=settings.NewMessageEmail,
                NewMessageInApp=settings.NewMessageInApp,
                MilestoneAddedEmail=settings.MilestoneAddedEmail,
                MilestoneAddedInApp=settings.MilestoneAddedInApp,
                ProposalReceivedEmail=settings.ProposalReceivedEmail,
                ProposalReceivedInApp=settings.ProposalReceivedInApp,
                WalletUpdatedEmail=settings.WalletUpdatedEmail,
                WalletUpdatedInApp=settings.WalletUpdatedInApp
            };
        }
        public static void UpdateEntity(this ClientNotificationSettings settings,UpdateNotificationClientDto dto)
        {
            settings.NewMessageInApp = dto.NewMessageInApp;
            settings.NewMessageEmail = dto.NewMessageEmail;

            settings.ProposalReceivedInApp = dto.ProposalReceivedInApp;
            settings.ProposalReceivedEmail = dto.ProposalReceivedEmail;

            settings.MilestoneAddedInApp = dto.MilestoneAddedInApp;
            settings.MilestoneAddedEmail = dto.MilestoneAddedEmail;

            settings.WalletUpdatedInApp = dto.WalletUpdatedInApp;
            settings.WalletUpdatedEmail = dto.WalletUpdatedEmail;
        }
    }
}
