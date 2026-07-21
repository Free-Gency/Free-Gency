using FreeGency.Api.Extensions;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Authentication.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController (IAuthServices authServices): ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto dto)
        {
            var result = await authServices.GetTokenAsync(dto);
            return result!.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto dto)
        {
            var result = await authServices.RegisterAsync(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequestDto dto)
        {
            var authResult = await authServices.GetRefeshTokenaync(dto.Token, dto.RefreshToken);
            return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
        }
        [HttpPut]
        public async Task<IActionResult> RevokeRefreshAsync([FromBody] RefreshTokenRequestDto dto)
        {
            var authResult = await authServices.RevokeRefeshTokenaync(dto.Token, dto.RefreshToken);
            return authResult.IsSuccess ? Ok() : authResult.ToProblem();
        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] ConfirmEmailRequestDto dto)
        {
            var result = await authServices.ComfirmEmail(dto);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPost("SendResetPassword")]
        public async Task<IActionResult> SendResetPassword([FromQuery] ResetPasswordRequestDto dto)
        {
            var result = await authServices.SendResetPasswordCode(dto);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpGet("ComfirmResetPassword")]
        public async Task<IActionResult> ComfirmResetPassword([FromQuery] ConfirmCodeRequestDto dto)
        {
            var result = await authServices.ComfirmCodeAsync(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();

        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordandConfirmPasswordRequestDto dto)
        {
            var result = await authServices.ResetNewPassword(dto);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

        }
    }
}
