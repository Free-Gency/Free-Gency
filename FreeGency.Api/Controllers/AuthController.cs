using FreeGency.Api.Extensions;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController (IAuthServices authServices): ControllerBase
    {
        [HttpPost("")]
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
    }
}
