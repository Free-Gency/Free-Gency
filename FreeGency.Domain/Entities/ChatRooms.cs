using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ChatRooms
    {
        public RoomType RoomType { get; set; }
        public Guid TeamId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ProposalId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string Title { get; set; }

    }
}
