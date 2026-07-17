using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.EmailFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IEmailAuthService
    {
        Task<Result<string>> SendEmail(SendEmailRequestDto dto);
    }
}
