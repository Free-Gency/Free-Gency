using FreeGency.Application.Features.LedgerEntryFeature.Dtos;
using FreeGency.Application.Features.LedgerEntryFeature.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.LedgerEntryFeature.Queries
{
    public class LedgerEntryService(ICurrentUserService currentUserService,IUnitOfWork unitOfWork) : ILedgerEntryService
    {
        private readonly IWalletRepository walletRepository = unitOfWork.Repository<IWalletRepository, Wallet>();
        private readonly ILedgerEntryRepository ledgerEntryRepository = unitOfWork.Repository<ILedgerEntryRepository, LedgerEntry>();
        public async Task<Result<PaginatedResult<LedgerEntryDto>>> GetLedgerEntryAsync(EntryFilter entryFilter)
        {
            var userId = currentUserService.UserId;
            var wallet = await walletRepository.GetByOwnerAsync(owner.User, userId);
            if (wallet == null) return Result.Failure<PaginatedResult<LedgerEntryDto>>(WalletErrors.NotFound);
            var query = ledgerEntryRepository.GetByWalletId(wallet.Id).Select(x=>x.ToDto());
            var result = await PaginatedResult<LedgerEntryDto>.CreateAsync(query, entryFilter.PageNumber, entryFilter.PageSize);
            return Result.Success(result);
        }

     
    }
}
