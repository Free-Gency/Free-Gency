using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class SavedProjects
    {
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }

    }
}
