using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ChatRoomMembers
    {
        public Guid ChatRoomId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime LastReadAt { get; set; }

    }
}
