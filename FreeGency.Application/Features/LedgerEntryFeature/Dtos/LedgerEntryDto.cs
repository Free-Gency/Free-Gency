using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.LedgerEntryFeature.Dtos
{
    public class LedgerEntryDto
    {
        public Guid Id { get; set; }

        public string EntryType { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? MilestoneId { get; set; }
    }
}
