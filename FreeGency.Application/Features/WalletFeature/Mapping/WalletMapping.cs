using FreeGency.Application.Features.WalletFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.WalletFeature.Mapping
{
    public static class WalletMapping
    {
        public static WalletUserDto ToDto(this Wallet wallet)
        {
            return new WalletUserDto
            {
                Id = wallet.Id,
                UserId = (Guid)wallet.OwnerUserId!,
                Currency = wallet.Currency,
                Reserved = wallet.Reserved,
                Available = wallet.Available,
                Pending = wallet.Pending
            };
        }
    }
}
