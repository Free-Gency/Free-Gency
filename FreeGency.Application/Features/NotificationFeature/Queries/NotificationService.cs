using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Application.Features.NotificationFeature.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.NotificationFeature.Commands
{
    public partial class NotificationService
    {
        private readonly IUserRepository userRepository = unitOfWork.Repository<IUserRepository, User>();
        public async Task<Result<PaginatedResult<NotificationDto>>> GetNotificationAsync(NotificationFilter notificationFilter)
        {
            var active = await userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active == null) return Result.Failure<PaginatedResult<NotificationDto>>(UserErrors.UserNotFound);
            var profileId = active.Value.ProfileId;
            var Notification = notificationRepository.GetNotificationAsync(profileId,currentUserService.UserId);
            var pagination = await PaginatedResult<NotificationDto>.CreateAsync(
                Notification.ToDto().OrderByDescending(x=>x.CreatedAt)
                ,notificationFilter.PageNumber, notificationFilter.PageSize);
            return Result.Success(pagination);
        }
        public async Task<Result<UnreadNotificationCountDto>> GetNotificationUnreadCount()
        {
            var userId = currentUserService.UserId;
            var active = await userRepository.GetActiveProfileAsync(userId);
            if (active == null) return Result.Failure<UnreadNotificationCountDto>(ProfileErrors.ProfileNotFound);
            var count = await  notificationRepository.GetNotificationAsync(active.Value.ProfileId, userId).CountAsync(x=>!x.IsRead);
            return Result.Success(new UnreadNotificationCountDto { count = count });
        }
    }
}
