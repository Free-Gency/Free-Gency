using FreeGency.Application.Features.LedgerEntryFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.LedgerEntryFeature.Mapping
{
    public static class LedgerEntryMapping
    {
        public static LedgerEntryDto ToDto(this LedgerEntry ledgerEntry)
        {
            return new LedgerEntryDto
            {
                Id = ledgerEntry.Id,
                CreatedAt = ledgerEntry.CreatedAt,
                Amount = ledgerEntry.Amount,
                Currency = ledgerEntry.Currency,
                MilestoneId = ledgerEntry.MilestoneId,
                ProjectId = ledgerEntry.ProjectId,
                EntryType = ledgerEntry.EntryType.ToString()
            };
        }
    }
}
