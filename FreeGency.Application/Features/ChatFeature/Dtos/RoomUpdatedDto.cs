using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class RoomUpdatedDto
    {
        public Guid RoomId { get; set; }
        public string? LastMessage { get; set; }
        public string? LastMessageType { get; set; }
        public DateTime LastMessageAt { get; set; }
        public string LastMessageSender { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
    }
}
