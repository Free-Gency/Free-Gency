using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ProjectEvents
    {
        public Guid ProjectId { get; set; }
        public Guid MilestoneId { get; set; }
        public Guid ActorUserId { get; set; }
        public EventType EventType { get; set; }
    }
}
