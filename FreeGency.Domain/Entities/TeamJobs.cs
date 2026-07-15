using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class TeamJobs
    {
        public Guid TeamId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TeamJobStatus Status { get; set; }
        public string CreatedByUserId { get;set;  }
        public DateTime ClosedAt { get; set; }
    }
}
