using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class TeamJoinRequests
    {
        public Guid TeamId { get; set; }
        public Guid TeamJobId { get; set; }
        public string UserId { get; set; }
        public string CoverLetter { get; set; }
        public string Job { get; set; }
        public TeamJoinRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime ResponseAt { get; set; }
        public string RespondedByUserId { get; set; }

    }
}
