using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class SavedProjects
    {
        public string UserId { get; set; }
        public Guid ProjectId { get; set; }

    }
}
