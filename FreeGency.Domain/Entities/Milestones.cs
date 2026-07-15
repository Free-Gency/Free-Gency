using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class Milestones
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal ReleasedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public int SortOrder { get; set; }
        //public 
    }
}
