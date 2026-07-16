using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ProjectMembers
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string RoleInProject { get; set; }
        public Guid AssignedByUserId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
