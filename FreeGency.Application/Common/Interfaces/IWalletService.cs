using FreeGency.Application.Features.WalletFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IWalletService
    {
        public Task<Result<WalletUserDto>> GetUserWallet();
        Task<Result<TopUpResponseDto>> CreateTopUpIntentAsync(TopUpRequestDto dto);
    }
}
