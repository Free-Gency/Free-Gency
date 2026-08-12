using FreeGency.Application.Features.NotificationFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task<Result<UnreadNotificationCountDto>> GetNotificationUnreadCount();
        Task CreateNotification(CreateNotificationRequest createNotificationRequest);
        Task<Result<PaginatedResult<NotificationDto>>> GetNotificationAsync(NotificationFilter filter);
        Task<Result> MarkAsSeen(Guid NotificationId);
    }
}
