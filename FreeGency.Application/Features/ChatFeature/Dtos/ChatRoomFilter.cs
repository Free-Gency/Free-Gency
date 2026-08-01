using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class ChatRoomFilter:PagedQuery
    {
        public RoomType? RoomType { get; set; }
        public ChatRoomStatus? Status { get; set; }
        public string? Search { get; set; }
    }
}
