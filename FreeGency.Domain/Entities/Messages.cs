using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class Messages
    {
        public Guid ChatRoomId { get; set; }
        public Guid SenderUserId { get; set; }
        public string Text { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }

    }
}
