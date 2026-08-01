using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class RoomMessagesDto
    {
        public Guid Id { get; set; }
        public Guid? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string MessageType { get; set; }
        public string? Text { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMine { get; set; }
    }
}
