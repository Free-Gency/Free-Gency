using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Features.WalletFeature.Dtos;
using FreeGency.Application.Features.WalletFeature.Mapping;
using FreeGency.Domain.Specifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.WalletFeature.Queries
{
    public partial class WalletService (ICurrentUserService currentUserService,IUnitOfWork unitOfWork,IOptions<StripeSetting> options,IHubContext<NotificationHub> hub): IWalletService
    {
        private readonly IWalletRepository walletRepository = unitOfWork.Repository<IWalletRepository, Wallet>();
        private readonly IPaymentTransactionRepository paymentTransactionRepository = unitOfWork.Repository<IPaymentTransactionRepository, PaymentTransaction>();
        private readonly ILedgerEntryRepository ledgerEntryRepository = unitOfWork.Repository<ILedgerEntryRepository, LedgerEntry>();
        private readonly StripeSetting _options = options.Value;
        private readonly IUserRepository userRepository = unitOfWork.Repository<IUserRepository, User>();
        public async Task<Result<WalletUserDto>> GetUserWallet()
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Result.Failure<WalletUserDto>(UserErrors.UserNotFound);
            var wallet = await walletRepository.GetEntityWithSpec(new WalletSpecification(userId));
            if(wallet==null) return Result.Failure<WalletUserDto>(UserErrors.UserNotFound);
            var response = wallet.ToDto();
            return Result.Success(response);
        }

       
    }
}
