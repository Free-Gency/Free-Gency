using FreeGency.Application.Features.Account.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IAccountService
    {
        Task<Result> UpdateClientProfileAsync(UpdateClientAccountDto dto);
        Task<Result<ClientAccountResponseDto>> GetClientProfile();
    }
}
