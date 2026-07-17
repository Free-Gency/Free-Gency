using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Authentication.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IAuthServices
    {
        Task<Result<AuthResponseDto>?> GetTokenAsync(LoginRequestDto dto);
        Task<Result> RegisterAsync(RegisterRequestDto dto);
        Task<Result<AuthResponseDto>> GetRefeshTokenaync(string Token, string RefreshToken, CancellationToken cancellationToken = default);
        Task<Result<bool>> RevokeRefeshTokenaync(string Token, string RefreshToken, CancellationToken cancellationToken = default);
        Task<Result<string>> ComfirmEmail(ConfirmEmailRequestDto dto);
        Task<Result<string>> SendResetPasswordCode(ResetPasswordRequestDto dto);
        Task<Result> ComfirmCodeAsync(ConfirmCodeRequestDto dto);
        Task<Result<string>> ResetNewPassword(ResetPasswordandConfirmPasswordRequestDto dto);

    }
}
 