using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.NotificationFeature.Dtos
{
    public class CreateNotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public string? Data { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public Guid? ClientProfileId { get; set; }
        public Guid? DeveloperProfileId { get; set; }

        // Context FKs
        public Guid? UserId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ProjectProposalId { get; set; }
        public Guid? TeamId { get; set; }
        public Guid? MilestoneId { get; set; }
        public Guid? ChatRoomId { get; set; }
        public Guid? MessageId { get; set; }
    }
}
