using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class TeamCategories
    {
        public Guid TeamId {  get; set; }
        public Guid CategoryId { get; set; }
        public bool IsPrimary { get; set; }
    }
}
