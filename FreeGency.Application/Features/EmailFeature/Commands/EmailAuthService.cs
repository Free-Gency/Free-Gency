using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.EmailFeature.Dtos;
using FreeGency.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.EmailFeature.Commands
{
    public class EmailAuthService(IEmailService emailService) : IEmailAuthService
    {
        public async Task<Result<string>> SendEmail(SendEmailRequestDto dto)
        {
            var response = await emailService.SendMassege(dto.email, dto.message, null);
            if (response) return Result.Success("Message sended");
            return Result.Failure<string>(EmailErrors.MessageNotSend);
        }
    }
}
