using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class TeamPayoutSplits
    {
        public Guid TeamId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public SplitType SplitType { get; set; }
        public decimal Value { get; set; }
    }
}
