using FreeGency.Api.Extensions;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Account.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController (IAccountService accountService): ControllerBase
    {
        [HttpGet("client/me")]
        public async Task<IActionResult> GetClientProfile()
        {
            var result = await accountService.GetClientProfile();
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpPut("client/me")]
        public async Task<IActionResult> UpdateClientProfile(UpdateClientAccountDto dto)
        {
            var result = await accountService.UpdateClientProfileAsync(dto);
            return result.IsSuccess ? Ok() : result.ToProblem();
        }
    }
}
