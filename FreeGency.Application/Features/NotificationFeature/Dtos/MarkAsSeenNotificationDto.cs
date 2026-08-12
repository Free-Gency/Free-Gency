using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.NotificationFeature.Dtos
{
    public class MarkAsSeenNotificationDto
    {
        public Guid NotificationId { get; set; }
        public bool WasUnread { get; set; }
    }
}
