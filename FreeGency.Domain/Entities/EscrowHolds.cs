using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class EscrowHolds
    {
        public Guid ProjectId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalReleased {  get; set; }
        // computed
        public decimal Remaining {  get; set; }
        public FundingStatus FundingStatus { get; set; }
        public PlanStatus planStatus { get; set; }
        public DateTime LockedAt { get; set; }
        public DateTime PlanAgreedAt { get; set; }
    }
}
