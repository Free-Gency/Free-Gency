using FreeGency.Application.Features.Account.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IAccountService
    {
        Task<Result> UpdateProfileAsync();
        Task<Result<ClientAccountResponseDto>> GetClientProfile();
    }
}
