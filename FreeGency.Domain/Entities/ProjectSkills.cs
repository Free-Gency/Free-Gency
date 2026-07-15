using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ProjectSkills
    {
        public Guid ProjectId { get; set; }
        public Guid SkillId { get; set; }
    }
}
