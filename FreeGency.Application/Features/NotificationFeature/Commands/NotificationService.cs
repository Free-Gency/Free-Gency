using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Features.EmailFeature.Dtos;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.NotificationFeature.Mapping;
using FreeGency.Infrastructure.Persistence.Repositories;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FreeGency.Application.Features.NotificationFeature.Commands
{
    public partial class NotificationService(ICurrentUserService currentUserService,IUnitOfWork unitOfWork,IHubContext<NotificationHub> hubContext,IEmailAuthService emailService) : INotificationService
    {
        private readonly INotificationRepository notificationRepository = unitOfWork.Repository<INotificationRepository, Notification>();
        private readonly IClientNotificationSettingsRepository clientNotificationSettingsRepository = unitOfWork.Repository<IClientNotificationSettingsRepository, ClientNotificationSettings>();
        private readonly IDeveloperNotificationSettingsRepository developerNotificationSettingsRepository = unitOfWork.Repository<IDeveloperNotificationSettingsRepository, DeveloperNotificationSettings>();
        private readonly IClientProfileRepository clientProfileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        private readonly IDeveloperProfileRepository developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        public async Task CreateNotification(CreateNotificationRequest createNotificationRequest)
        {
            var notification = createNotificationRequest.ToEntity();
            await notificationRepository.AddAsync(notification);
            await unitOfWork.SaveChangesAsync();
            if(createNotificationRequest.UserId!=null)
            {
                var active = await userRepository.GetActiveProfileAsync(createNotificationRequest.UserId.Value);
                if (active.Value.Mode == profileMode.Client)
                {
                    var settings = await clientNotificationSettingsRepository
                        .GetByProfileId(active.Value.ProfileId);

                    if (ShouldPush(settings, createNotificationRequest.Type))
                    {
                        var connections = NotificationHub.GetConnections(active.Value.ProfileId);
                        if (connections.Count > 0)
                            await hubContext.Clients.Clients(connections).SendAsync("NotificationCreated", notification.ToDto());
                    }

                    if (ShouldSendEmail(settings, createNotificationRequest.Type))
                    {
                        var Email = await clientProfileRepository.GetEmail(active.Value.ProfileId);
                        if (Email != null)
                            BackgroundJob.Enqueue(() => emailService.SendEmail(new SendEmailRequestDto
                            {
                                email = Email,
                                message = notification.Body
                            }));
                            
                    }
                }
                else
                {
                    var settings = await developerNotificationSettingsRepository
                        .GetDeveloperNotification(active.Value.ProfileId);

                    if (settings is null || ShouldPush(settings, createNotificationRequest.Type))
                    {
                        var connections = NotificationHub.GetConnections(active.Value.ProfileId);
                        if (connections.Count > 0)
                            await hubContext.Clients.Clients(connections).SendAsync("NotificationCreated", notification.ToDto());
                    }

                    if (settings is not null && ShouldSendEmail(settings, createNotificationRequest.Type))
                    {
                        var email = await developerProfileRepository
                        .GetEmail(active.Value.ProfileId);

                        if (email != null)
                        {
                            BackgroundJob.Enqueue(() => emailService.SendEmail(new SendEmailRequestDto
                            {
                                email = email,
                                message = notification.Body
                            }));
                        }
                    }
                }
            }
            else if (createNotificationRequest.ClientProfileId != null)
            {
                var clientNotification = await clientNotificationSettingsRepository.GetByProfileId(createNotificationRequest.ClientProfileId.Value);
                if (clientNotification is null || ShouldPush(clientNotification, createNotificationRequest.Type))
                {
                    var connections = NotificationHub.GetConnections(createNotificationRequest.ClientProfileId.Value);
                    if (connections.Count > 0)
                        await hubContext.Clients.Clients(connections).SendAsync("NotificationCreated", notification.ToDto());
                }
                if (clientNotification is not null && ShouldSendEmail(clientNotification, createNotificationRequest.Type))
                {
                    var Email = await clientProfileRepository.GetEmail(createNotificationRequest.ClientProfileId.Value);
                    if (Email != null)
                        BackgroundJob.Enqueue(() => emailService.SendEmail(new SendEmailRequestDto
                        {
                            email = Email,
                            message = notification.Body
                        }));
                }
            }
            else if (createNotificationRequest.DeveloperProfileId != null)
            {
                var developerNotification = await developerNotificationSettingsRepository.GetDeveloperNotification(createNotificationRequest.DeveloperProfileId.Value);
                if (developerNotification is null || ShouldPush(developerNotification, createNotificationRequest.Type))
                {
                    var connections = NotificationHub.GetConnections(createNotificationRequest.DeveloperProfileId.Value);
                    if(connections.Count>0)
                        await hubContext.Clients.Clients(connections).SendAsync("NotificationCreated", notification.ToDto()); 
                }
                if (developerNotification is not null && ShouldSendEmail(developerNotification, createNotificationRequest.Type))
                {
                    var email = await developerProfileRepository
                        .GetEmail(createNotificationRequest.DeveloperProfileId.Value);

                    if (email != null)
                    {
                        BackgroundJob.Enqueue(() => emailService.SendEmail(new SendEmailRequestDto
                        {
                            email = email,
                            message = notification.Body
                        }));
                    }
                }
            }
        }
        private static bool ShouldPush(
      ClientNotificationSettings settings,
      NotificationType type)
        {
            return type switch
            {
                NotificationType.NewChatMessage
                    => settings.NewMessageInApp,

                NotificationType.NewProposal
                or NotificationType.ProposalAccepted
                or NotificationType.ProposalRejected
                    => settings.ProposalReceivedInApp,

                NotificationType.MilestonePlanProposed
                or NotificationType.MilestonePlanChangesRequested
                or NotificationType.MilestonePlanAgreed
                or NotificationType.EscrowLocked
                or NotificationType.MilestoneSubmitted
                or NotificationType.MilestoneChangesRequested
                or NotificationType.MilestoneApproved
                or NotificationType.MilestoneReleased
                or NotificationType.MilestoneFunded
                    => settings.MilestoneAddedInApp,

                NotificationType.Wallet
                    => settings.WalletUpdatedInApp,

                // دول معندكش Setting ليهم دلوقتي
                NotificationType.JoinRequestReceived
                or NotificationType.JoinRequestAccepted
                or NotificationType.JoinRequestRejected
                or NotificationType.ProjectPublished
                or NotificationType.ReviewReminder
                or NotificationType.System
                    => true,

                _ => true
            };
        }
        private static bool ShouldSendEmail(
    ClientNotificationSettings settings,
    NotificationType type)
        {
            return type switch
            {
                NotificationType.NewChatMessage
                    => settings.NewMessageEmail,

                NotificationType.NewProposal
                or NotificationType.ProposalAccepted
                or NotificationType.ProposalRejected
                    => settings.ProposalReceivedEmail,

                NotificationType.MilestonePlanProposed
                or NotificationType.MilestonePlanChangesRequested
                or NotificationType.MilestonePlanAgreed
                or NotificationType.EscrowLocked
                or NotificationType.MilestoneSubmitted
                or NotificationType.MilestoneChangesRequested
                or NotificationType.MilestoneApproved
                or NotificationType.MilestoneReleased
                or NotificationType.MilestoneFunded
                    => settings.MilestoneAddedEmail,

                NotificationType.Wallet
                    => settings.WalletUpdatedEmail,

                // دول معندكش Settings ليهم
                NotificationType.JoinRequestReceived
                or NotificationType.JoinRequestAccepted
                or NotificationType.JoinRequestRejected
                or NotificationType.ProjectPublished
                or NotificationType.ReviewReminder
                or NotificationType.System
                    => false,

                _ => false
            };
        }
        private static bool ShouldPush(
    DeveloperNotificationSettings settings,
    NotificationType type)
        {
            return type switch
            {
                NotificationType.NewChatMessage
                    => settings.MessagesInApp,

                NotificationType.NewProposal
                or NotificationType.ProposalAccepted
                or NotificationType.ProposalRejected
                or NotificationType.ProjectPublished
                    => settings.ProjectsInApp,

                NotificationType.MilestonePlanProposed
                or NotificationType.MilestonePlanChangesRequested
                or NotificationType.MilestonePlanAgreed
                or NotificationType.EscrowLocked
                or NotificationType.MilestoneSubmitted
                or NotificationType.MilestoneChangesRequested
                or NotificationType.MilestoneApproved
                or NotificationType.MilestoneReleased
                or NotificationType.MilestoneFunded

                    => settings.MilestonesInApp,

                NotificationType.Wallet
                    => settings.WalletInApp,

                NotificationType.JoinRequestReceived
                or NotificationType.JoinRequestAccepted
                or NotificationType.JoinRequestRejected
                    => settings.TeamsInApp,

                NotificationType.ReviewReminder
                or NotificationType.System
                    => true,

                _ => true
            };
        }
        private static bool ShouldSendEmail(
    DeveloperNotificationSettings settings,
    NotificationType type)
        {
            return type switch
            {
                NotificationType.NewChatMessage
                    => settings.MessagesEmail,

                NotificationType.NewProposal
                or NotificationType.ProposalAccepted
                or NotificationType.ProposalRejected
                or NotificationType.ProjectPublished
                    => settings.ProjectsEmail,

                NotificationType.MilestonePlanProposed
                or NotificationType.MilestonePlanChangesRequested
                or NotificationType.MilestonePlanAgreed
                or NotificationType.EscrowLocked
                or NotificationType.MilestoneSubmitted
                or NotificationType.MilestoneChangesRequested
                or NotificationType.MilestoneApproved
                or NotificationType.MilestoneReleased
                or NotificationType.MilestoneFunded

                    => settings.MilestonesEmail,

                NotificationType.Wallet
                    => settings.WalletEmail,

                NotificationType.JoinRequestReceived
                or NotificationType.JoinRequestAccepted
                or NotificationType.JoinRequestRejected
                    => settings.TeamsEmail,

                NotificationType.ReviewReminder
                or NotificationType.System
                    => false,

                _ => false
            };
        }

      
    }
}
