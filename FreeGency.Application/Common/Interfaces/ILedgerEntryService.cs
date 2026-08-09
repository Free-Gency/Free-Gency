using FreeGency.Application.Features.LedgerEntryFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface ILedgerEntryService
    {
        Task< Result<PaginatedResult<LedgerEntryDto>>> GetLedgerEntryAsync(EntryFilter entryFilter);
    }
}
