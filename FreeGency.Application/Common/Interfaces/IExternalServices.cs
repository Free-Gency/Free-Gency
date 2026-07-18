using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IExternalServices
    {
        Task<Result<AuthResponseDto>> LoginWithGoogleAsync();
    }
}
