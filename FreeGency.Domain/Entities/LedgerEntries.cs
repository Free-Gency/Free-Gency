using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class LedgerEntries
    {
        public Guid WalletId { get; set; }
        public EntryType EntryType { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public Guid ProjectId { get; set;  }
        public Guid MilestoneId { get; set; }
        public string IdempotencyKey { get; set; }
        public string PaymentProviderRef { get; set; }

    }
}
