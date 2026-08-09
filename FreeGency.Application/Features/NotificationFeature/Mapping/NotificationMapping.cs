using FreeGency.Application.Features.NotificationFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.NotificationFeature.Mapping
{
    public static class NotificationMapping
    {
        public static Notification ToEntity(this CreateNotificationRequest request)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Body = request.Body,
                Type = request.Type,
                ImageUrl = request.ImageUrl,
                ActionUrl = request.ActionUrl,
                Data = request.Data,
                IsRead = request.IsRead,
                ReadAt = request.ReadAt,
                UserId=request.UserId,
                ClientProfileId = request.ClientProfileId,
                DeveloperProfileId = request.DeveloperProfileId,

                ProjectId = request.ProjectId,
                ProjectProposalId = request.ProjectProposalId,
                TeamId = request.TeamId,
                MilestoneId = request.MilestoneId,
                ChatRoomId = request.ChatRoomId,
                MessageId = request.MessageId,

                CreatedAt = DateTime.UtcNow
            };
        }
        public static NotificationDto ToDto(this Notification n)
        {
            return new NotificationDto
            {
                Id=n.Id,
                Title = n.Title,
                Body = n.Body,
                Type = n.Type.ToString(),
                ImageUrl = n.ImageUrl,
                ActionUrl = n.ActionUrl,
                Data = n.Data,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            };
        }
        public static IQueryable< NotificationDto> ToDto(this IQueryable<Notification> notifications)
        {
            return notifications.Select(n => new NotificationDto
            {
                Id=n.Id,
                Title = n.Title,
                Body = n.Body,
                Type = n.Type.ToString(),
                ImageUrl = n.ImageUrl,
                ActionUrl = n.ActionUrl,
                Data = n.Data,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            });
        }
    }
}
