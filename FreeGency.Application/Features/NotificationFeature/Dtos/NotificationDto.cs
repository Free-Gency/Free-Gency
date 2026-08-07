using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.NotificationFeature.Dtos
{
    public class NotificationDto
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Type { get; set; }

        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public string? Data { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
