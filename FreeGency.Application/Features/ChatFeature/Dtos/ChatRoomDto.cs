using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class ChatRoomDto
    {
        public Guid Id { get; set; }
        public string RoomType { get; set; }
        public string Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public string? LastMessageType { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessageSender { get; set; }
    }
}
