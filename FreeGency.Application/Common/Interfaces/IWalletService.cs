using FreeGency.Application.Features.WalletFeature.Dtos;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IWalletService
    {
        public Task<Result<WalletUserDto>> GetUserWallet();
        Task<Result<TopUpResponseDto>> CreateTopUpIntentAsync(TopUpRequestDto dto);
        Task<Result> HandleSucceeded(PaymentIntent @object);
        Task<Result> HandleFailed(PaymentIntent @object);
        Task<Result> HandleCanceled(PaymentIntent @object);
    }
}
