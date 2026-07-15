using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class TeamMembers
    {
        public Guid TeamId { get; set; }
        public string UserId { get; set; }

        public Role TeamRole { get; set; }

        public string Job { get; set; }
        public string JoinedAt { get; set; }

    }
}
